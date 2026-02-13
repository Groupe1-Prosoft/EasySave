using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

namespace EasyLog
{
    /// <summary>
    /// Writes JSON or XML logs to a daily file.
    /// </summary>
    public class Logger : ILogger
    {
        private string logFilePath;
        private string logFormat;

        /// <summary>
        /// Initializes the logger with the desired log format.
        /// </summary>
        public Logger(string logFormat)
        {
            this.logFormat = string.Equals(logFormat, "xml", StringComparison.OrdinalIgnoreCase) ? "xml" : "json";
            logFilePath = CreateDailyLogFile();
        }

        /// <summary>
        /// Creates the daily log file path based on the current format.
        /// </summary>
        public string CreateDailyLogFile()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string logsPath = Path.Combine(appData, "EasySave", "Logs");
            Directory.CreateDirectory(logsPath);

            string extension = logFormat == "xml" ? "xml" : "json";
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "." + extension;
            return Path.Combine(logsPath, fileName);
        }

        /// <summary>
        /// Writes a log entry to the daily file.
        /// </summary>
        public bool WriteLog(LogData data)
        {
            try
            {
                logFilePath = CreateDailyLogFile();

                if (logFormat == "xml")
                {
                    List<LogData> logs = new();

                    if (File.Exists(logFilePath))
                    {
                        using var read = File.OpenRead(logFilePath);
                        if (read.Length > 0)
                        {
                            var serializer = new XmlSerializer(typeof(List<LogData>));
                            logs = serializer.Deserialize(read) as List<LogData> ?? new List<LogData>();
                        }
                    }

                    logs.Add(data);

                    using var write = File.Create(logFilePath);
                    var xmlSerializer = new XmlSerializer(typeof(List<LogData>));
                    xmlSerializer.Serialize(write, logs);
                    return true;
                }

                List<LogData> jsonLogs = new();

                if (File.Exists(logFilePath))
                {
                    string existing = File.ReadAllText(logFilePath);
                    if (!string.IsNullOrWhiteSpace(existing))
                    {
                        jsonLogs = JsonSerializer.Deserialize<List<LogData>>(existing) ?? new List<LogData>();
                    }
                }

                jsonLogs.Add(data);

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(logFilePath, JsonSerializer.Serialize(jsonLogs, options));

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}