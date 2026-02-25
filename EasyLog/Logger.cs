using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EasyLog
{
    /// <summary>
    /// Writes JSON or XML logs to a local file or a remote server.
    /// </summary>
    public class Logger : ILogger
    {
        private string logFilePath;
        private string logFormat;
        private string logMode;
        private string serverUrl;
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly object _lock = new();

        /// <summary>
        /// Initializes the logger with format, mode and server URL.
        /// </summary>
        public Logger(string logFormat, string logMode = "local", string serverUrl = "")
        {
            this.logFormat = string.Equals(logFormat, "xml", StringComparison.OrdinalIgnoreCase) ? "xml" : "json";
            this.logMode = logMode.ToLower();
            this.serverUrl = serverUrl;
            logFilePath = CreateDailyLogFile();
        }

        /// <summary>
        /// Creates the daily log file path.
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
        /// Entry point to write a log based on the selected mode.
        /// </summary>
        public bool WriteLog(LogData data)
        {
            bool localSuccess = true;
            bool remoteSuccess = true;

            // Set the computer name to identify the user on the server.
            data.ClientName = Environment.MachineName;

            // Handle local logging.
            if (logMode == "local" || logMode == "both")
            {
                localSuccess = WriteLocalLog(data);
            }

            // Handle centralized logging to Docker.
            if (logMode == "centralized" || logMode == "both")
            {
                remoteSuccess = SendToRemoteServer(data).Result;
            }

            return localSuccess && remoteSuccess;
        }

        /// <summary>
        /// Writes the log entry to a local file.
        /// </summary>
        private bool WriteLocalLog(LogData data)
        {
            lock (_lock)
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

        /// <summary>
        /// Sends the log entry to the Docker server using HTTP.
        /// </summary>
        private async Task<bool> SendToRemoteServer(LogData data)
        {
            try
            {
                if (string.IsNullOrEmpty(serverUrl)) return false;

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{serverUrl}/logs", content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}