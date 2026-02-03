using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasyLog
{
    // --- PARTIE 1 : LE CONTRAT (INTERFACE) ---
    // C'est ça qui valide le "D" de SOLID (Dependency Inversion).
    // L'application ne dépendra plus de la classe, mais de ce contrat.
    public interface ILogger
    {
        bool WriteLog(LogData data);
    }

    // --- PARTIE 2 : LE MODÈLE DE DONNÉES (MVVM : Model) ---
    // C'est un pur objet de données (DTO), parfait pour le MVVM.
    public class LogData
    {
        public required DateTime Timestamp { get; set; }
        public required string Name { get; set; }
        public required string Source { get; set; }
        public required string Target { get; set; }
        public long Size { get; set; }
        public long TransferTime { get; set; }

        public string ToJSON() => JsonSerializer.Serialize(this);

        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Source) &&
                   !string.IsNullOrWhiteSpace(Target);
        }
    }

    // --- PARTIE 3 : LE MOTEUR (SERVICE) ---
    // Cette classe implémente l'interface ILogger.
    public class Logger : ILogger
    {
        private string _logFilePath;

        public Logger()
        {
            _logFilePath = CreateDailyLogFile();
        }

        // Cette méthode reste publique si besoin, ou peut passer privée
        // si seule l'interface est utilisée. On la garde publique pour l'instant.
        public string CreateDailyLogFile()
        {
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "Logs");

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            return Path.Combine(directoryPath, DateTime.Now.ToString("yyyy-MM-dd") + ".json");
        }

        // Implémentation de la méthode définie dans l'interface
        public bool WriteLog(LogData data)
        {
            try
            {
                List<LogData> logs = new List<LogData>();

                if (File.Exists(_logFilePath))
                {
                    string jsonContent = File.ReadAllText(_logFilePath);
                    if (!string.IsNullOrWhiteSpace(jsonContent))
                    {
                        logs = JsonSerializer.Deserialize<List<LogData>>(jsonContent) ?? new List<LogData>();
                    }
                }

                logs.Add(data);

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(_logFilePath, JsonSerializer.Serialize(logs, options));

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}