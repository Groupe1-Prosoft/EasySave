using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EasySave.Models
{
    /// <summary>
    /// Stores the runtime configuration and manages persisted backup jobs.
    /// </summary>
    public class Configuration
    {
        private const string configFilePath = "config.json";
        private readonly List<BackupJob> jobs = new();
        private string logFormat = "json";

        /// <summary>
        /// Gets or sets the log format.
        /// </summary>
        public string LogFormat
        {
            get => logFormat;
            set => logFormat = string.IsNullOrWhiteSpace(value) ? "json" : value.ToLower();
        }

        /// <summary>
        /// Returns a copy of the current job list.
        /// </summary>
        public List<BackupJob> GetJobs()
        {
            return new List<BackupJob>(jobs);
        }

        /// <summary>
        /// Adds a job if the limit is not reached.
        /// </summary>
        public bool AddJob(BackupJob job)
        {
            if (!job.Validate()) return false;

            if (job.Id == 0)
            {
                int nextId = jobs.Count == 0 ? 1 : jobs.Max(j => j.Id) + 1;
                job.Id = nextId;
            }

            jobs.Add(job);
            return SaveConfig();
        }

        /// <summary>
        /// Removes a job by its identifier.
        /// </summary>
        public bool RemoveJob(int id)
        {
            var job = jobs.FirstOrDefault(j => j.Id == id);
            if (job == null) return false;
            jobs.Remove(job);
            return SaveConfig();
        }

        /// <summary>
        /// Loads configuration and jobs from disk.
        /// </summary>
        public bool LoadConfig()
        {
            try
            {
                string path = GetConfigFullPath();
                if (!File.Exists(path)) return true;

                string json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<ConfigurationData>(json);

                jobs.Clear();
                if (data?.Jobs != null) jobs.AddRange(data.Jobs);
                logFormat = string.IsNullOrWhiteSpace(data?.LogFormat) ? "json" : data.LogFormat.ToLower();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Saves configuration and jobs to disk.
        /// </summary>
        public bool SaveConfig()
        {
            try
            {
                string path = GetConfigFullPath();
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                var data = new ConfigurationData
                {
                    Jobs = jobs,
                    LogFormat = logFormat
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(path, JsonSerializer.Serialize(data, options));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetConfigFullPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "EasySave", configFilePath);
        }

        private sealed class ConfigurationData
        {
            public List<BackupJob> Jobs { get; set; } = new();
            public string LogFormat { get; set; } = "json";
        }
    }
}