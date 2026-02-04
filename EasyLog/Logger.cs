using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasyLog
{
    // 1. L'INTERFACE (Le contrat obligatoire pour SOLID)
    public interface ILogger
    {
        bool WriteLog(LogData data);
    }

    // 2. LE MODÈLE DE DONNÉES (La structure du JSON)
    public class LogData
    {
        public string? Name { get; set; }
        public string? Source { get; set; }
        public string? Target { get; set; }
        public long Size { get; set; }
        public long TransferTime { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // 3. LA CLASSE PRINCIPALE (Le moteur)
    public class Logger : ILogger
    {
        private string _logFilePath;

        public Logger()
        {
            // On définit le dossier : AppData/EasySave/Logs
            string directoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EasySave",
                "Logs"
            );

            // Si le dossier n'existe pas, on le crée
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Le nom du fichier est la date d'aujourd'hui (ex: 2023-10-25.json)
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".json";
            _logFilePath = Path.Combine(directoryPath, fileName);
        }

        public bool WriteLog(LogData data)
        {
            try
            {
                // Liste temporaire pour lire le fichier existant
                List<LogData> logs = new List<LogData>();

                // Si le fichier existe déjà, on récupère son contenu
                if (File.Exists(_logFilePath))
                {
                    string existingJson = File.ReadAllText(_logFilePath);
                    // On vérifie que le fichier n'est pas vide pour éviter de planter
                    if (!string.IsNullOrWhiteSpace(existingJson))
                    {
                        try
                        {
                            logs = JsonSerializer.Deserialize<List<LogData>>(existingJson) ?? new List<LogData>();
                        }
                        catch
                        {
                            // Si le JSON est corrompu, on repart sur une liste vide
                            logs = new List<LogData>();
                        }
                    }
                }

                // On ajoute le nouveau log
                logs.Add(data);

                // On sauvegarde le tout avec une jolie mise en forme (Indented)
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonOutput = JsonSerializer.Serialize(logs, options);

                File.WriteAllText(_logFilePath, jsonOutput);

                return true;
            }
            catch (Exception ex)
            {
                // En cas de pépin (disque plein, droits d'accès...)
                Console.WriteLine($"[Logger Error] {ex.Message}");
                return false;
            }
        }
    }
}