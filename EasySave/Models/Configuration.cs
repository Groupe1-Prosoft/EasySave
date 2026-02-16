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

        private string businessSoftwareName = string.Empty;

        // New variables for encryption settings
        private string cryptoSoftPath = string.Empty;
        private List<string> extensionsToEncrypt = new();

        /// <summary>
        /// Gets or sets the name of the business software to monitor.
        /// </summary>
        public string GetBusinessSoftwareName()
        {
            return businessSoftwareName;
        }

        public void SetBusinessSoftwareName(string name)
        {
            businessSoftwareName = name;
        }

        /// <summary>
        /// Gets or sets the log format.
        /// </summary>
        public string LogFormat
        {
            get => logFormat;
            set => logFormat = string.IsNullOrWhiteSpace(value) ? "json" : value.ToLower();
        }

        // Gets or sets the path to CryptoSoft executable
        public string CryptoSoftPath
        {
            get => cryptoSoftPath;
            set => cryptoSoftPath = value;
        }

        // Gets or sets the list of extensions to encrypt
        public List<string> ExtensionsToEncrypt
        {
            get => extensionsToEncrypt;
            set => extensionsToEncrypt = value;
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
                businessSoftwareName = data?.BusinessSoftwareName ?? string.Empty;

                // Load encryption settings
                cryptoSoftPath = data?.CryptoSoftPath ?? string.Empty;
                extensionsToEncrypt = data?.ExtensionsToEncrypt ?? new List<string>();

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
                    LogFormat = logFormat,
                    BusinessSoftwareName = businessSoftwareName,
                    // Save encryption settings
                    CryptoSoftPath = cryptoSoftPath,
                    ExtensionsToEncrypt = extensionsToEncrypt
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
            public string BusinessSoftwareName { get; set; } = string.Empty;

            // New properties for JSON storage
            public string CryptoSoftPath { get; set; } = string.Empty;
            public List<string> ExtensionsToEncrypt { get; set; } = new();
        }
    }
}