using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasyLog
{
    public class LogEntry
    {
        public required string Timestamp { get; set; }     // Obligatoire
        public required string BackupName { get; set; }    // Obligatoire
        public required string SourcePath { get; set; }    // Obligatoire
        public required string TargetPath { get; set; }    // Obligatoire
        public long FileSize { get; set; }
        public double TransferTime { get; set; }
    }
    public static class Logger
    {
        public static void WriteLog(LogEntry entry)
        {
            // Emplacement sécurisé (pas de c:\temp\)
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "Logs");
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".json";
            string filePath = Path.Combine(directoryPath, fileName);

            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

            List<LogEntry> logs = new List<LogEntry>();
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                logs = JsonSerializer.Deserialize<List<LogEntry>>(json) ?? new List<LogEntry>();
            }

            logs.Add(entry);

            // Formatage JSON avec retours à la ligne (pagination) pour Notepad
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(filePath, JsonSerializer.Serialize(logs, options));
        }
    }
}