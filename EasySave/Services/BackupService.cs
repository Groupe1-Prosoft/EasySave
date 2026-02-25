using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using EasyLog;
using EasySave.Models;


namespace EasySave.Services
{
    public class BackupService : IBackupService
    {
        private readonly ILogger _logger;
        private readonly Configuration _configuration;
        private readonly CryptoSoftService _cryptoSoftService;
        private readonly BusinessSoftwareMonitor _businessMonitor;
        private readonly ManualResetEventSlim _pauseEvent = new(true);
        private CancellationTokenSource? _stopCts;
        private BackupState? _currentState;
        private bool _stopRequested;
        private bool _isRunning;

        // Semaphores to manage concurrency
        private readonly SemaphoreSlim _largeFileSemaphore = new SemaphoreSlim(1, 1);

        // Dependencies are injected to decouple object creation
        public BackupService(
            Configuration configuration,
            ILogger logger,
            CryptoSoftService cryptoSoftService,
            BusinessSoftwareMonitor businessMonitor)
        {
            _configuration = configuration;
            _logger = logger;
            _cryptoSoftService = cryptoSoftService;
            _businessMonitor = businessMonitor;
        }

        public bool IsPaused => !_pauseEvent.IsSet;

        public bool IsRunning => _isRunning;

        public bool IsStopping => _stopRequested;

        public void Pause()
        {
            if (!_isRunning || IsPaused) return;
            _pauseEvent.Reset();
            UpdateState("PAUSE");
        }

        public void Resume()
        {
            if (!_isRunning || !IsPaused) return;
            _pauseEvent.Set();
            UpdateState("ACTIF");
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _stopRequested = true;
            _stopCts?.Cancel();
            _pauseEvent.Set();
            UpdateState("ARRETE");
        }

        public bool ExecuteJob(BackupJob job)
        {
            InitializeExecution();

            try
            {
                _cryptoSoftService.SetPath(_configuration.CryptoSoftPath);
                _businessMonitor.SetProcessName(_configuration.GetBusinessSoftwareName());

                if (!job.Validate()) return false;

                // Stop execution if business software is detected
                if (_businessMonitor.IsRunning())
                {
                    var blockLog = new LogData
                    {
                        Timestamp = DateTime.Now,
                        Name = job.Name ?? string.Empty,
                        Source = job.SourceDir ?? string.Empty,
                        Target = job.TargetDir ?? string.Empty,
                        Size = 0,
                        TransferTime = 0,
                        EncryptionTime = -1
                    };
                    _logger.WriteLog(blockLog);
                    return false;
                }

                if (!Directory.Exists(job.TargetDir)) Directory.CreateDirectory(job.TargetDir!);

                var files = GetFileList(job.SourceDir!);
                long totalSize = CalculateTotalSize(files);

                _currentState = new BackupState
                {
                    JobName = job.Name,
                    Timestamp = DateTime.Now,
                    State = "ACTIF",
                    TotalFiles = files.Count,
                    TotalSize = totalSize,
                    FilesRemaining = files.Count,
                    SizeRemaining = totalSize,
                    Progression = 0
                };

                _currentState.UpdateStateJSON();

                int processed = 0;
                foreach (var file in files)
                {
                    // Check for pause or stop requests before processing file
                    if (!WaitIfPausedOrStopped()) return false;

                    string relative = Path.GetRelativePath(job.SourceDir!, file);
                    string targetFile = Path.Combine(job.TargetDir!, relative);

                    if (job.Type == BackupType.Differential && File.Exists(targetFile))
                    {
                        if (File.GetLastWriteTimeUtc(file) <= File.GetLastWriteTimeUtc(targetFile))
                        {
                            processed++;
                            _currentState.FilesRemaining--;
                            _currentState.SizeRemaining -= new FileInfo(file).Length;
                            UpdateProgress(processed, files.Count);
                            continue;
                        }
                    }

                    long transferTime;
                    long fileSizeBytes = new FileInfo(file).Length;
                    long limitBytes = _configuration.MaxLargeFileSizeKB * 1024;
                    bool isLarge = limitBytes > 0 && fileSizeBytes > limitBytes;

                    // Copy file (using large file semaphore if needed)
                    if (isLarge)
                    {
                        _largeFileSemaphore.Wait();
                        try
                        {
                            transferTime = CopyFile(file, targetFile);
                        }
                        finally
                        {
                            _largeFileSemaphore.Release();
                        }
                    }
                    else
                    {
                        transferTime = CopyFile(file, targetFile);
                    }

                    long encryptionTime = 0;

                    // Encrypt file using a lock to prevent concurrent access (Task 7)
                    if (_cryptoSoftService.IsEligible(targetFile, _configuration.ExtensionsToEncrypt))
                    {
                        _cryptoSoftService.AcquireLock();
                        try
                        {
                            long time = _cryptoSoftService.EncryptFile(targetFile);
                            if (time >= 0) encryptionTime = time;
                        }
                        finally
                        {
                            _cryptoSoftService.ReleaseLock();
                        }
                    }

                    var data = new LogData
                    {
                        Timestamp = DateTime.Now,
                        Name = job.Name ?? string.Empty,
                        Source = file,
                        Target = targetFile,
                        Size = new FileInfo(file).Length,
                        TransferTime = transferTime,
                        EncryptionTime = encryptionTime
                    };

                    _logger.WriteLog(data);

                    processed++;
                    _currentState.FilesRemaining--;
                    _currentState.SizeRemaining -= data.Size;
                    UpdateProgress(processed, files.Count);
                }

                if (_stopRequested)
                {
                    UpdateState("ARRETE");
                    return false;
                }

                UpdateState("NON ACTIF");
                return true;
            }
            finally
            {
                _isRunning = false;
                _pauseEvent.Set();
                _stopCts?.Dispose();
                _stopCts = null;
            }
        }

        public bool ExecuteSequential(List<int> ids)
        {
            bool success = true;
            foreach (int id in ids)
            {
                if (_stopRequested) break;

                var jobs = _configuration.GetJobs();
                var job = jobs.FirstOrDefault(j => j.Id == id);
                if (job != null)
                {
                    bool result = ExecuteJob(job);
                    success &= result;

                    if (_stopRequested) break;
                }
            }
            return success && !_stopRequested;
        }
        // TODO: implement true parallel execution with BackupJobController (one Task per job)
        public bool ExecuteParallel(List<int> ids)
        {
            return ExecuteSequential(ids);
        }

        // TODO: implement per-job control via _controllers[id] (BackupJobController)
        public void PauseJob(int id) => Pause();
        public void ResumeJob(int id) => Resume();
        public void StopJob(int id) => Stop();

        public void PauseAll() => Pause();
        public void ResumeAll() => Resume();
        public void StopAll() => Stop();

        private long CopyFile(string source, string target)
        {
            var stopwatch = Stopwatch.StartNew();
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(source, target, overwrite: true);
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private List<string> GetFileList(string sourceDir)
        {
            if (!Directory.Exists(sourceDir)) return new List<string>();
            return Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories).ToList();
        }

        private long CalculateTotalSize(List<string> files)
        {
            return files.Sum(f => new FileInfo(f).Length);
        }

        private void UpdateProgress(int processed, int total)
        {
            if (_currentState == null) return;
            _currentState.Progression = (int)((processed / (double)total) * 100);
            _currentState.UpdateStateJSON();
        }

        private void InitializeExecution()
        {
            _stopRequested = false;
            _isRunning = true;
            _pauseEvent.Set();
            _stopCts?.Dispose();
            _stopCts = new CancellationTokenSource();
        }

        private bool WaitIfPausedOrStopped()
        {
            if (_stopRequested) return false;
            try
            {
                _pauseEvent.Wait(_stopCts?.Token ?? CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            return !_stopRequested;
        }

        private void UpdateState(string state)
        {
            if (_currentState == null) return;
            _currentState.State = state;
            _currentState.Timestamp = DateTime.Now;
            _currentState.UpdateStateJSON();
        }
    }
}