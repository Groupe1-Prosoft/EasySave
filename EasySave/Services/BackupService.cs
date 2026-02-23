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
    /// <summary>
    /// Manages backup execution and logging.
    /// Dependencies are NOW INJECTED (not created inside constructor).
    /// This enables true Dependency Injection: BackupService doesn't control object creation.
    /// </summary>
    public class BackupService
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

        /// <summary>
        /// Initializes the service with ALL dependencies injected (not created inside).
        /// This is TRUE Dependency Injection: all objects come from outside.
        /// Goal: Decouple BackupService from object creation responsibility.
        /// </summary>
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

        /// <summary>
        /// Gets whether the backup is currently paused.
        /// </summary>
        public bool IsPaused => !_pauseEvent.IsSet;

        /// <summary>
        /// Gets whether a backup is currently running.
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Gets whether a stop request has been issued.
        /// </summary>
        public bool IsStopping => _stopRequested;

        /// <summary>
        /// Pauses the current backup execution.
        /// </summary>
        public void Pause()
        {
            if (!_isRunning || IsPaused) return;
            _pauseEvent.Reset();
            UpdateState("PAUSE");
        }

        /// <summary>
        /// Resumes the current backup execution.
        /// </summary>
        public void Resume()
        {
            if (!_isRunning || !IsPaused) return;
            _pauseEvent.Set();
            UpdateState("ACTIF");
        }

        /// <summary>
        /// Requests to stop the current backup execution.
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;
            _stopRequested = true;
            _stopCts?.Cancel();
            _pauseEvent.Set();
            UpdateState("ARRETE");
        }

        /// <summary>
        /// Executes a backup job.
        /// </summary>
        public bool ExecuteJob(BackupJob job)
        {
            InitializeExecution();

            try
            {
                _cryptoSoftService.SetPath(_configuration.CryptoSoftPath);
                _businessMonitor.SetProcessName(_configuration.GetBusinessSoftwareName());

                if (!job.Validate()) return false;

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

                    long transferTime = CopyFile(file, targetFile);
                    long encryptionTime = 0;

                    if (_cryptoSoftService.IsEligible(targetFile, _configuration.ExtensionsToEncrypt))
                    {
                        long time = _cryptoSoftService.EncryptFile(targetFile);
                        if (time >= 0) encryptionTime = time;
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

        /// <summary>
        /// Executes several jobs sequentially by their identifiers.
        /// </summary>
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

        /// <summary>
        /// Copies a single file and returns the transfer time in milliseconds.
        /// </summary>
        private long CopyFile(string source, string target)
        {
            var stopwatch = Stopwatch.StartNew();
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(source, target, overwrite: true);
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Returns all files in the source directory (recursive).
        /// </summary>
        private List<string> GetFileList(string sourceDir)
        {
            if (!Directory.Exists(sourceDir)) return new List<string>();
            return Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories).ToList();
        }

        /// <summary>
        /// Calculates total size of all files in the list.
        /// </summary>
        private long CalculateTotalSize(List<string> files)
        {
            return files.Sum(f => new FileInfo(f).Length);
        }

        /// <summary>
        /// Updates backup progress.
        /// </summary>
        private void UpdateProgress(int processed, int total)
        {
            if (_currentState == null) return;
            _currentState.Progression = (int)((processed / (double)total) * 100);
            _currentState.UpdateStateJSON();
        }

        /// <summary>
        /// Initializes execution control state.
        /// </summary>
        private void InitializeExecution()
        {
            _stopRequested = false;
            _isRunning = true;
            _pauseEvent.Set();
            _stopCts?.Dispose();
            _stopCts = new CancellationTokenSource();
        }

        /// <summary>
        /// Waits if paused, or exits if a stop request was issued.
        /// </summary>
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

        /// <summary>
        /// Updates the current state and persists it.
        /// </summary>
        private void UpdateState(string state)
        {
            if (_currentState == null) return;
            _currentState.State = state;
            _currentState.Timestamp = DateTime.Now;
            _currentState.UpdateStateJSON();
        }
    }
}