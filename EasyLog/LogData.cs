using System;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

namespace EasyLog
{
    /// <summary>
    /// Represents a log entry for a file transfer.
    /// </summary>
    public class LogData
    {
        /// <summary>
        /// Gets or sets the timestamp of the log.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the job name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source file path.
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target file path.
        /// </summary>
        public string Target { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the transfer time in milliseconds.
        /// </summary>
        public long TransferTime { get; set; }

        /// <summary>
        /// Gets or sets the encryption time in milliseconds (0 = none, >0 = time, &lt;0 = error).
        /// </summary>
        public long EncryptionTime { get; set; }

        /// <summary>
        /// Serializes the log entry to JSON.
        /// </summary>
        public string ToJSON()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }

        /// <summary>
        /// Serializes the log entry to XML.
        /// </summary>
        public string ToXML()
        {
            var serializer = new XmlSerializer(typeof(LogData));
            using var writer = new StringWriter();
            serializer.Serialize(writer, this);
            return writer.ToString();
        }
    }
}