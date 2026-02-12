using System;
using System.Text.Json;

namespace EasyLog
{
    /// <summary>
    /// Represents one log record of a file transfer.
    /// </summary>
    public class LogData
    {
        /// <summary>
        /// Gets or sets the backup job name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the source file path (UNC).
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Gets or sets the target file path (UNC).
        /// </summary>
        public string? Target { get; set; }

        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the transfer time in milliseconds (negative on error).
        /// </summary>
        public long TransferTime { get; set; }

        /// <summary>
        /// Gets or sets the log timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Serializes the current log entry to JSON.
        /// </summary>
        public string ToJSON()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }

        /// <summary>
        /// Validates that required fields are present.
        /// </summary>
        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(Source)
                && !string.IsNullOrWhiteSpace(Target);
        }
    }
}