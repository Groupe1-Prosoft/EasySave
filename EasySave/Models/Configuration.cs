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

        // Private attributes (as seen in the diagram)
        private readonly List<BackupJob> jobs = new();
        private string logFormat = "json";
        private string businessSoftwareName = string.Empty;

        // Note: The diagram calls this private attribute 'encryptExtensions'
        private List<string> encryptExtensions = new();
        private readonly object _lock = new();

        // We keep CryptoSoftPath to make it work (required implementation)
        public string CryptoSoftPath { get; set; } = string.Empty;

        // --- Methods matching the diagram ---

        public string GetBusinessSoftwareName()
        {
            return businessSoftwareName;
        }

        public void SetBusinessSoftwareName(string name)
        {
            businessSoftwareName = name;
        }

        public string GetLogFormat()
        {
            return logFormat;
        }

        // We keep the LogFormat property for JSON binding, but add the Setter from the diagram
        public void SetLogFormat(string format)
        {
            logFormat = string.IsNullOrWhiteSpace(format) ? "json" : format.ToLower();
        }

        // C# Property to make JSON serialization easier, 
        // but we use the methods below to respect the diagram.
        public string LogFormat
        {
            get => GetLogFormat();
            set => SetLogFormat(value);
        }

        /// <summary>
        /// Matches the diagram: Returns the list of extensions.
        /// </summary>
        public List<string> GetEncryptExtensions()
        {
            return encryptExtensions;
        }

        /// <summary>
        /// Matches the diagram: Sets the list of extensions.
        /// </summary>
        public void SetEncryptExtensions(List<string> ext)
        {
            encryptExtensions = ext;
        }

        // Wrapper property for JSON serialization (JsonSerializer needs public properties)
        public List<string> ExtensionsToEncrypt
        {
            get => encryptExtensions;
            set => encryptExtensions = value;
        }

        public List<BackupJob> GetJobs()
        {
            return new List<BackupJob>(jobs);
        }

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

        public bool RemoveJob(int id)
        {
            var job = jobs.FirstOrDefault(j => j.Id == id);
            if (job == null) return false;
            jobs.Remove(job);
            return SaveConfig();
        }

        public bool LoadConfig()
        {
            lock (_lock)
            {
                try
                {
                    string path = GetConfigFullPath();
                    if (!File.Exists(path)) return true;

                    string json = File.ReadAllText(path);
                    var data = JsonSerializer.Deserialize<ConfigurationData>(json);

                    jobs.Clear();
                    if (data?.Jobs != null) jobs.AddRange(data.Jobs);

                    SetLogFormat(data?.LogFormat ?? "json");
                    SetBusinessSoftwareName(data?.BusinessSoftwareName ?? string.Empty);

                    // Loading extensions and CryptoSoft path
                    CryptoSoftPath = data?.CryptoSoftPath ?? string.Empty;
                    SetEncryptExtensions(data?.ExtensionsToEncrypt ?? new List<string>());

                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool SaveConfig()
        {
            lock (_lock)
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
                        CryptoSoftPath = CryptoSoftPath,
                        ExtensionsToEncrypt = encryptExtensions
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
        }
        private static string GetConfigFullPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "EasySave", configFilePath);
        }

        // Internal class for the JSON structure
        private sealed class ConfigurationData
        {
            public List<BackupJob> Jobs { get; set; } = new();
            public string LogFormat { get; set; } = "json";
            public string BusinessSoftwareName { get; set; } = string.Empty;
            public string CryptoSoftPath { get; set; } = string.Empty;
            public List<string> ExtensionsToEncrypt { get; set; } = new();
        }
    }
}