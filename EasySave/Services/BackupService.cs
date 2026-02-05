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
    public class BackupService
    {
        public List<BackupJob> Jobs { get; set; }
        private readonly ILogger _logger;

        private readonly string _jobsFilePath;
        private readonly string _stateFilePath;

        private readonly LanguageManager _lang;

        public BackupService(LanguageManager lang)
        {
            _lang = lang;
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave");

            // Create the directory if it does not exist
            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);

            _jobsFilePath = Path.Combine(appDataPath, "jobs.json");
            _stateFilePath = Path.Combine(appDataPath, "state.json");

            _logger = new EasyLog.Logger();
            LoadJobs();
        }

        // Methods to manage backup jobs

        public bool AddJob(BackupJob job)
        {
            // Limit to 5 jobs max
            if (Jobs.Count >= 5) return false;
            Jobs.Add(job);
            SaveJobs();
            return true;
        }

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

        private void SaveJobs()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(_jobsFilePath, JsonSerializer.Serialize(Jobs, options));
            }
            catch (Exception ex) { Console.WriteLine($"Error saving jobs: {ex.Message}"); }
        }

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

        // Methods for execution and state management

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

            // Initialize the state of the backup
            var state = new BackupState
            {
                JobName = job.Name,
                Timestamp = DateTime.Now,
                State = "ACTIF",
                SourceDirectory = job.SourceDirectory,
                TargetDirectory = job.TargetDirectory
            };

            // Calculate total files and size
            CalculateTotals(job.SourceDirectory, state);

            // Save the initial state
            UpdateStateFile(state);

            Console.WriteLine(_lang.GetText("Processing", job.Name, state.TotalFiles));

            // Start the copy process
            CopyDirectory(job.SourceDirectory, job.TargetDirectory, job, state);

            // End of the job
            state.State = "NON ACTIF";
            state.CurrentSourceFile = "";
            state.CurrentTargetFile = "";

            state.Timestamp = DateTime.Now;
            UpdateStateFile(state);
        }

        private void CalculateTotals(string path, BackupState state)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);

                // Count all files recursively
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

        private void CopyDirectory(string sourceDir, string targetDir, BackupJob job, BackupState state)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(targetDir, file.Name);

                // Differential backup logic
                if (job.Type == BackupType.Differential && File.Exists(targetFilePath))
                {
                    FileInfo destFile = new FileInfo(targetFilePath);

                    // If source file is older or same date as destination we skip it
                    if (file.LastWriteTime <= destFile.LastWriteTime)
                    {
                        // Update counters because the file is skipped
                        state.FilesRemaining--;
                        state.SizeRemaining -= file.Length;
                        if (state.SizeRemaining < 0) state.SizeRemaining = 0;

                        // Update progression before skipping the file
                        state.Progression = state.TotalFiles > 0
                            ? (double)(state.TotalFiles - state.FilesRemaining) / state.TotalFiles * 100
                            : 0;

                        state.Timestamp = DateTime.Now;

                        // Save state to keep the progress bar accurate
                        UpdateStateFile(state);

                        continue;
                    }
                }

                long startTime = DateTime.Now.Ticks;

                // Update state before copy
                state.CurrentSourceFile = file.FullName;
                state.CurrentTargetFile = targetFilePath;
                state.State = "ACTIF";

                state.Timestamp = DateTime.Now;

                UpdateStateFile(state);

                try
                {
                    // Copy the file and overwrite if exists
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

                    // Log the error with negative time
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

                // Update progression after copying the file
                state.Progression = state.TotalFiles > 0
                    ? (double)(state.TotalFiles - state.FilesRemaining) / state.TotalFiles * 100
                    : 0;

                state.Timestamp = DateTime.Now;
                UpdateStateFile(state);

                // Prevent negative size values
                if (state.SizeRemaining < 0) state.SizeRemaining = 0;
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newTargetDir = Path.Combine(targetDir, subDir.Name);

                // Create subdirectory if it does not exist
                if (!Directory.Exists(newTargetDir)) Directory.CreateDirectory(newTargetDir);
                CopyDirectory(subDir.FullName, newTargetDir, job, state);
            }
        }

        // Method to update the state.json file
        private void UpdateStateFile(BackupState currentState)
        {
            try
            {
                List<BackupState> states = new List<BackupState>();

                // Read existing state file
                if (File.Exists(_stateFilePath))
                {
                    string json = File.ReadAllText(_stateFilePath);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        states = JsonSerializer.Deserialize<List<BackupState>>(json) ?? new List<BackupState>();
                    }
                }

                // Check if the job is already in the list
                var existingState = states.FirstOrDefault(s => s.JobName == currentState.JobName);
                if (existingState != null)
                {
                    states.Remove(existingState);
                }

                // Add the new state
                states.Add(currentState);

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(_stateFilePath, JsonSerializer.Serialize(states, options));
            }
            catch
            {
               
            }
        }

        // Helper to convert local path to UNC path
        private string ToUncPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            if (path.StartsWith(@"\\"))
                return path;

            string machineName = Environment.MachineName;

            // UNC format for windows drives
            if (path.Length >= 2 && path[1] == ':')
            {
                return $@"\\{machineName}\{path[0]}${path.Substring(2)}";
            }

            // UNC format for relative paths
            return $@"\\{machineName}{path}";
        }
    }
}