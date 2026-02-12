using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using EasySave.Models;
using EasyLog;
using EasySave.Localization;

namespace EasySave.Services
{
    /// <summary>
    /// Manages backup jobs, state tracking, and file transfer logging.
    /// </summary>
    public class BackupService
    {
        /// <summary>
        /// Gets or sets the in-memory list of backup jobs.
        /// </summary>
        public List<BackupJob> Jobs { get; set; }

        private readonly ILogger _logger;
        private readonly string _jobsFilePath;
        private readonly string _stateFilePath;
        private readonly LanguageManager _lang;

        /// <summary>
        /// Initializes the service, prepares AppData paths, and loads jobs.
        /// </summary>
        public BackupService(LanguageManager lang)
        {
            _lang = lang;
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave");

            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);

            _jobsFilePath = Path.Combine(appDataPath, "jobs.json");
            _stateFilePath = Path.Combine(appDataPath, "state.json");

            _logger = new EasyLog.Logger();
            LoadJobs();
        }

        /// <summary>
        /// Adds a job and persists it, enforcing the 5-job limit.
        /// </summary>
        public bool AddJob(BackupJob job)
        {
            if (Jobs.Count >= 5) return false;
            Jobs.Add(job);
            SaveJobs();
            return true;
        }

        /// <summary>
        /// Deletes a job by index and persists the updated list.
        /// </summary>
        public bool DeleteJob(int index)
        {
            int realIndex = index - 1;
            if (realIndex >= 0 && realIndex < Jobs.Count)
            {
                Jobs.RemoveAt(realIndex);
                SaveJobs();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Writes jobs.json with formatted JSON.
        /// </summary>
        private void SaveJobs()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(_jobsFilePath, JsonSerializer.Serialize(Jobs, options));
            }
            catch (Exception ex) { Console.WriteLine($"Error saving jobs: {ex.Message}"); }
        }

        /// <summary>
        /// Loads jobs.json into the in-memory list.
        /// </summary>
        private void LoadJobs()
        {
            try
            {
                if (File.Exists(_jobsFilePath))
                {
                    string json = File.ReadAllText(_jobsFilePath);
                    Jobs = JsonSerializer.Deserialize<List<BackupJob>>(json) ?? new List<BackupJob>();
                }
                else { Jobs = new List<BackupJob>(); }
            }
            catch { Jobs = new List<BackupJob>(); }
        }

        /// <summary>
        /// Executes a job, updates state.json in real time, and logs file transfers.
        /// </summary>
        public void ExecuteJob(BackupJob job)
        {
            if (!Directory.Exists(job.SourceDirectory))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(_lang.GetText("SourceNotFound", job.SourceDirectory));
                Console.ResetColor();
                return;
            }
            if (!Directory.Exists(job.TargetDirectory)) Directory.CreateDirectory(job.TargetDirectory);

            var state = new BackupState
            {
                JobName = job.Name,
                Timestamp = DateTime.Now,
                State = "ACTIF",
                SourceDirectory = job.SourceDirectory,
                TargetDirectory = job.TargetDirectory
            };

            CalculateTotals(job.SourceDirectory, state);

            UpdateStateFile(state);

            Console.WriteLine(_lang.GetText("Processing", job.Name, state.TotalFiles));

            CopyDirectory(job.SourceDirectory, job.TargetDirectory, job, state);

            state.State = "NON ACTIF";
            state.CurrentSourceFile = "";
            state.CurrentTargetFile = "";

            state.Timestamp = DateTime.Now;
            UpdateStateFile(state);
        }

        /// <summary>
        /// Computes total file count and size for the given source path.
        /// </summary>
        private void CalculateTotals(string path, BackupState state)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);

                var files = dir.GetFiles("*", SearchOption.AllDirectories);

                state.TotalFiles = files.Length;
                state.FilesRemaining = files.Length;

                long size = 0;
                foreach (var f in files) size += f.Length;

                state.TotalSize = size;
                state.SizeRemaining = size;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating totals: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively copies directories and logs each file transfer.
        /// Supports differential copy behavior.
        /// </summary>
        private void CopyDirectory(string sourceDir, string targetDir, BackupJob job, BackupState state)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(targetDir, file.Name);

                if (job.Type == BackupType.Differential && File.Exists(targetFilePath))
                {
                    FileInfo destFile = new FileInfo(targetFilePath);

                    if (file.LastWriteTime <= destFile.LastWriteTime)
                    {
                        state.FilesRemaining--;
                        state.SizeRemaining -= file.Length;
                        if (state.SizeRemaining < 0) state.SizeRemaining = 0;

                        state.Progression = state.TotalFiles > 0
                            ? (double)(state.TotalFiles - state.FilesRemaining) / state.TotalFiles * 100
                            : 0;

                        state.Timestamp = DateTime.Now;

                        UpdateStateFile(state);

                        continue;
                    }
                }

                long startTime = DateTime.Now.Ticks;

                state.CurrentSourceFile = file.FullName;
                state.CurrentTargetFile = targetFilePath;
                state.State = "ACTIF";

                state.Timestamp = DateTime.Now;

                UpdateStateFile(state);

                try
                {
                    file.CopyTo(targetFilePath, true);
                    long timeMs = (DateTime.Now.Ticks - startTime) / 10000;

                    var logData = new LogData
                    {
                        Name = job.Name,
                        Source = ToUncPath(file.FullName),
                        Target = ToUncPath(targetFilePath),
                        Size = file.Length,
                        TransferTime = timeMs,
                        Timestamp = DateTime.Now
                    };
                    _logger.WriteLog(logData);
                    Console.WriteLine(_lang.GetText("FileCopied", file.Name));
                }
                catch (Exception ex)
                {
                    long timeMs = (DateTime.Now.Ticks - startTime) / 10000;

                    var logData = new LogData
                    {
                        Name = job.Name,
                        Source = ToUncPath(file.FullName),
                        Target = ToUncPath(targetFilePath),
                        Size = file.Length,
                        TransferTime = -timeMs,
                        Timestamp = DateTime.Now
                    };
                    _logger.WriteLog(logData);
                    Console.WriteLine(_lang.GetText("CopyError", ex.Message));
                }

                state.FilesRemaining--;
                state.SizeRemaining -= file.Length;

                state.Progression = state.TotalFiles > 0
                    ? (double)(state.TotalFiles - state.FilesRemaining) / state.TotalFiles * 100
                    : 0;

                state.Timestamp = DateTime.Now;
                UpdateStateFile(state);

                if (state.SizeRemaining < 0) state.SizeRemaining = 0;
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newTargetDir = Path.Combine(targetDir, subDir.Name);

                if (!Directory.Exists(newTargetDir)) Directory.CreateDirectory(newTargetDir);
                CopyDirectory(subDir.FullName, newTargetDir, job, state);
            }
        }

        /// <summary>
        /// Writes or updates the job entry in state.json.
        /// </summary>
        private void UpdateStateFile(BackupState currentState)
        {
            try
            {
                List<BackupState> states = new List<BackupState>();

                if (File.Exists(_stateFilePath))
                {
                    string json = File.ReadAllText(_stateFilePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        states = JsonSerializer.Deserialize<List<BackupState>>(json) ?? new List<BackupState>();
                    }
                }

                var existingState = states.FirstOrDefault(s => s.JobName == currentState.JobName);
                if (existingState != null)
                {
                    states.Remove(existingState);
                }

                states.Add(currentState);

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(_stateFilePath, JsonSerializer.Serialize(states, options));
            }
            catch
            {
            }
        }

        /// <summary>
        /// Converts a local path to a UNC-like path using the machine name.
        /// </summary>
        private string ToUncPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            if (path.StartsWith(@"\\"))

                return path;

            string machineName = Environment.MachineName;

            if (path.Length >= 2 && path[1] == ':')
            {
                return $@"\\{machineName}\{path[0]}${path.Substring(2)}";
            }

            return $@"\\{machineName}{path}";
        }
    }
}