using System;
using System.IO;

namespace EasyLog
{
    public class Logger : ILogger
    {
        private string _logDirectory;
        private string _logFormat; // Format for the logs (xml or json)

        // Constructor accepting the log directory and the format
        public Logger(string logDirectory, string logFormat)
        {
            _logDirectory = logDirectory;
            // Force the format to lowercase to avoid errors
            _logFormat = logFormat.ToLower();

            // Create the directory if it does not exist
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void WriteLog(LogData logData)
        {
            // Choose the strategy based on the format
            if (_logFormat == "xml")
            {
                // XML
                string filePath = Path.Combine(_logDirectory, "logs.xml");
                string content = logData.ToXML();

                // Append content to the file
                File.AppendAllText(filePath, content + Environment.NewLine);
            }
            else
            {
                // JSON 
                string filePath = Path.Combine(_logDirectory, "logs.json");
                string content = logData.ToJSON();

                // Append content to the file
                File.AppendAllText(filePath, content + Environment.NewLine);
            }
        }
    }
}