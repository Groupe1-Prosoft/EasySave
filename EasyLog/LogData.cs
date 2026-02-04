using System;
using System.Text.Json;

namespace EasyLog
{
    public class LogData
    {
        // Properties defined in the class diagram
        public string? Name { get; set; }
        public string? Source { get; set; }
        public string? Target { get; set; }
        public long Size { get; set; }
        public long TransferTime { get; set; }
        public DateTime Timestamp { get; set; }

        // Method to convert the object to a JSON string
        public string ToJSON()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }

        // Method to validate that essential data is present
        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(Source)
                && !string.IsNullOrWhiteSpace(Target);
        }
    }
}