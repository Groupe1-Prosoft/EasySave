using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasyLog
{
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

    public class Logger
    {
        // Exactement comme sur le diagramme : "LogFilePath"
        private string LogFilePath;

        public Logger()
        {
            LogFilePath = CreateDailyLogFile();
        }

        public string CreateDailyLogFile()
        {
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "Logs");

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            return Path.Combine(directoryPath, DateTime.Now.ToString("yyyy-MM-dd") + ".json");
        }

        public bool WriteLog(LogData data)
        {
            try
            {
                List<LogData> logs = new List<LogData>();

                if (File.Exists(LogFilePath))
                {
                    string jsonContent = File.ReadAllText(LogFilePath);
                    if (!string.IsNullOrWhiteSpace(jsonContent))
                    {
                        logs = JsonSerializer.Deserialize<List<LogData>>(jsonContent) ?? new List<LogData>();
                    }
                }

                logs.Add(data);

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(LogFilePath, JsonSerializer.Serialize(logs, options));

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}