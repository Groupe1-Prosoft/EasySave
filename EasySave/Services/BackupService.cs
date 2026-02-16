using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Models;

namespace EasySave.Services
{
    /// <summary>
    /// Manages backup execution and logging.
    /// </summary>
    public class BackupService
    {
        private readonly ILogger logger;
        private readonly Configuration configuration;

        // Respecting the class diagram: explicit dependency
        private readonly CryptoSoftService cryptoSoftService;
        private readonly BusinessSoftwareMonitor businessMonitor;
        private BackupState? currentState;

        /// <summary>
        /// Initializes the service with configuration.
        /// </summary>
        public BackupService(Configuration configuration)
        {
            this.configuration = configuration;
            logger = new Logger(configuration.LogFormat);

            // Initialize the sub-services
            businessMonitor = new BusinessSoftwareMonitor();
            businessMonitor.SetProcessName(configuration.GetBusinessSoftwareName());

            cryptoSoftService = new CryptoSoftService();
            cryptoSoftService.SetPath(configuration.CryptoSoftPath);
        }

        /// <summary>
        /// Executes a backup job.
        /// </summary>
        public bool ExecuteJob(BackupJob job)
        {
            // Update CryptoSoft path in case config changed at runtime
            cryptoSoftService.SetPath(configuration.CryptoSoftPath);
            businessMonitor.SetProcessName(configuration.GetBusinessSoftwareName());

            if (!job.Validate()) return false;

            businessMonitor.SetProcessName(configuration.GetBusinessSoftwareName());


            if (businessMonitor.IsRunning())
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
                logger.WriteLog(blockLog);
                return false;
            }

            if (!Directory.Exists(job.TargetDir)) Directory.CreateDirectory(job.TargetDir!);

            var files = GetFileList(job.SourceDir!);
            long totalSize = CalculateTotalSize(files);

            currentState = new BackupState
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

            currentState.UpdateStateJSON();

            int processed = 0;
            foreach (var file in files)
            {
                string relative = Path.GetRelativePath(job.SourceDir!, file);
                string targetFile = Path.Combine(job.TargetDir!, relative);

                if (job.Type == BackupType.Differential && File.Exists(targetFile))
                {
                    if (File.GetLastWriteTimeUtc(file) <= File.GetLastWriteTimeUtc(targetFile))
                    {
                        processed++;
                        currentState.FilesRemaining--;
                        currentState.SizeRemaining -= new FileInfo(file).Length;
                        UpdateProgress(processed, files.Count);
                        continue;
                    }
                }

                // New logic compliant with Class Diagram
                long transferTime = CopyFile(file, targetFile);
                long encryptionTime = 0;

                // Check eligibility using the service
                if (cryptoSoftService.IsEligible(targetFile, configuration.ExtensionsToEncrypt))
                {
                    // Encrypt using the service
                    long time = cryptoSoftService.EncryptFile(targetFile);
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

                logger.WriteLog(data);

                processed++;
                currentState.FilesRemaining--;
                currentState.SizeRemaining -= data.Size;
                UpdateProgress(processed, files.Count);
            }

            currentState.State = "NON ACTIF";
            currentState.Timestamp = DateTime.Now;
            currentState.UpdateStateJSON();

            return true;
        }

        /// <summary>
        /// Executes several jobs sequentially by their identifiers.
        /// </summary>
        public bool ExecuteSequential(List<int> ids)
        {
            bool success = true;
            var jobs = configuration.GetJobs();

            foreach (int id in ids)
            {
                var job = jobs.FirstOrDefault(j => j.Id == id);
                if (job == null)
                {
                    success = false;
                    continue;
                }
                businessMonitor.SetProcessName(configuration.GetBusinessSoftwareName());


                if (businessMonitor.IsRunning()) break;

                if (!ExecuteJob(job)) success = false;

            }

            return success;
        }

        private long CopyFile(string source, string dest)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                Stopwatch stopwatch = Stopwatch.StartNew();
                File.Copy(source, dest, true);
                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            }
            catch
            {
                return -1;
            }
        }

        private List<string> GetFileList(string directory)
        {
            return Directory.GetFiles(directory, "*", SearchOption.AllDirectories).ToList();
        }

        private long CalculateTotalSize(List<string> files)
        {
            long size = 0;
            foreach (var file in files)
            {
                size += new FileInfo(file).Length;
            }
            return size;
        }

        private void UpdateProgress(int current, int total)
        {
            if (currentState == null) return;
            currentState.Progression = total == 0 ? 0 : (int)((current * 100.0) / total);
            currentState.Timestamp = DateTime.Now;
            currentState.UpdateStateJSON();
        }
    }
}