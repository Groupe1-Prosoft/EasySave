using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasyLog
{
    public class Logger : ILogger
    {
        private string _logFilePath;

        public Logger()
        {
            // Set the file path when the logger starts
            _logFilePath = CreateDailyLogFile();
        }

        // Generates the daily log file path (e.g., 2024-02-04.json)
        public string CreateDailyLogFile()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "Logs");

            // Create directory if it does not exist
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".json";
            return Path.Combine(appDataPath, fileName);
        }

        public bool WriteLog(LogData data)
        {
            // Validation check before writing
            if (!data.Validate()) return false;

            try
            {
                // Update path in case the date changed during execution
                _logFilePath = CreateDailyLogFile();

                List<LogData> logs = new List<LogData>();

                // Read existing logs if the file exists
                if (File.Exists(_logFilePath))
                {
                    string existingJson = File.ReadAllText(_logFilePath);
                    if (!string.IsNullOrWhiteSpace(existingJson))
                    {
                        logs = JsonSerializer.Deserialize<List<LogData>>(existingJson) ?? new List<LogData>();
                    }
                }

                // Add the new log entry
                logs.Add(data);

                // Write everything back to the file with formatting
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(_logFilePath, JsonSerializer.Serialize(logs, options));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logger Error] {ex.Message}");
                return false;
            }
        }
    }
}