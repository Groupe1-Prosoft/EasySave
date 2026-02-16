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

        private BackupState? currentState;
        private readonly BusinessSoftwareMonitor businessMonitor;


        /// <summary>
        /// Initializes the service with configuration.
        /// </summary>
        public BackupService(Configuration configuration)
        {
            this.configuration = configuration;
            logger = new Logger(configuration.LogFormat);

            businessMonitor = new BusinessSoftwareMonitor();
            businessMonitor.SetProcessName(configuration.GetBusinessSoftwareName());
        }

        /// <summary>
        /// Executes a backup job.
        /// </summary>
        public bool ExecuteJob(BackupJob job)
        {
            if (!job.Validate()) return false;

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
                    EncryptionTime = -1 // -1 means error or blocked
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

                // Call CopyFile which now returns both transfer time and encryption time
                (long transferTime, long encryptionTime) = CopyFile(file, targetFile);

                var data = new LogData
                {
                    Timestamp = DateTime.Now,
                    Name = job.Name ?? string.Empty,
                    Source = file,
                    Target = targetFile,
                    Size = new FileInfo(file).Length,
                    TransferTime = transferTime,
                    EncryptionTime = encryptionTime // New property for logs
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

                if (businessMonitor.IsRunning())
                {
                    // Log the block event for this job
                    break;
                }


                if (!ExecuteJob(job))
                {
                    success = false;
                }
            }

            return success;
        }

        // Modified method to handle encryption
        private (long TransferTime, long EncryptionTime) CopyFile(string source, string dest)
        {
            long encryptionTime = 0; // 0 means no encryption
            long transferTime = 0;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                Stopwatch stopwatch = Stopwatch.StartNew();

                // Check if encryption is needed
                string extension = Path.GetExtension(source);
                bool needEncryption = configuration.ExtensionsToEncrypt.Contains(extension)
                                      && File.Exists(configuration.CryptoSoftPath);

                if (needEncryption)
                {
                    // Prepare CryptoSoft process
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = configuration.CryptoSoftPath,
                        Arguments = $"\"{source}\" \"{dest}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = Process.Start(startInfo))
                    {
                        process.WaitForExit();
                    }

                    stopwatch.Stop();
                    // Total time is considered both transfer and encryption time here
                    transferTime = stopwatch.ElapsedMilliseconds;
                    encryptionTime = stopwatch.ElapsedMilliseconds;
                }
                else
                {
                    // Standard copy
                    File.Copy(source, dest, true);
                    stopwatch.Stop();
                    transferTime = stopwatch.ElapsedMilliseconds;
                    encryptionTime = 0; // 0 because not encrypted
                }

                return (transferTime, encryptionTime);
            }
            catch
            {
                // In case of error, return negative values
                return (-1, -1);
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