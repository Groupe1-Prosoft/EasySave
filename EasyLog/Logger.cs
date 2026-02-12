using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasyLog
{
    /// <summary>
    /// Writes JSON logs to a daily file under AppData.
    /// </summary>
    public class Logger : ILogger
    {
        private string _logFilePath;

        /// <summary>
        /// Initializes the logger and resolves the daily file path.
        /// </summary>
        public Logger()
        {
            _logFilePath = CreateDailyLogFile();
        }

        /// <summary>
        /// Creates the daily log file path (yyyy-MM-dd.json).
        /// </summary>
        public string CreateDailyLogFile()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "Logs");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".json";
            return Path.Combine(appDataPath, fileName);
        }

        /// <summary>
        /// Appends a log entry to the daily JSON log file.
        /// </summary>
        public bool WriteLog(LogData data)
        {
            if (!data.Validate()) return false;

            try
            {
                _logFilePath = CreateDailyLogFile();

                List<LogData> logs = new List<LogData>();

                if (File.Exists(_logFilePath))
                {
                    string existingJson = File.ReadAllText(_logFilePath);
                    if (!string.IsNullOrWhiteSpace(existingJson))
                    {
                        logs = JsonSerializer.Deserialize<List<LogData>>(existingJson) ?? new List<LogData>();
                    }
                }

                logs.Add(data);

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