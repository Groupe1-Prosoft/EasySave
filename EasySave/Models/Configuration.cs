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

        // Attributs privés (comme sur le diagramme)
        private readonly List<BackupJob> jobs = new();
        private string logFormat = "json";
        private string businessSoftwareName = string.Empty;

        // Note: Le diagramme nomme ceci 'encryptExtensions' en privé
        private List<string> encryptExtensions = new();

        // On garde CryptoSoftPath pour que ça marche (implémentation nécessaire)
        public string CryptoSoftPath { get; set; } = string.Empty;

        // --- Méthodes conformes au diagramme ---

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

        // On garde la propriété LogFormat pour le binding JSON, mais on ajoute le Setter du diagramme
        public void SetLogFormat(string format)
        {
            logFormat = string.IsNullOrWhiteSpace(format) ? "json" : format.ToLower();
        }

        // Propriété Property C# pour faciliter la sérialisation JSON, 
        // mais on utilise les méthodes ci-dessous pour respecter le diagramme.
        public string LogFormat
        {
            get => GetLogFormat();
            set => SetLogFormat(value);
        }

        /// <summary>
        /// Conforme au diagramme : Retourne la liste des extensions.
        /// </summary>
        public List<string> GetEncryptExtensions()
        {
            return encryptExtensions;
        }

        /// <summary>
        /// Conforme au diagramme : Définit la liste des extensions.
        /// </summary>
        public void SetEncryptExtensions(List<string> ext)
        {
            encryptExtensions = ext;
        }

        // Propriété wrapper pour la sérialisation JSON (JsonSerializer a besoin de propriétés publiques)
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

                // Chargement des extensions et du chemin CryptoSoft
                CryptoSoftPath = data?.CryptoSoftPath ?? string.Empty;
                SetEncryptExtensions(data?.ExtensionsToEncrypt ?? new List<string>());

                return true;
            }
            catch
            {
                return false;
            }
        }

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

        private static string GetConfigFullPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "EasySave", configFilePath);
        }

        // Classe interne pour la structure JSON
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