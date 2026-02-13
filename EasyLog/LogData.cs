using System;
using System.IO;
using System.Text.Json;        // Library for JSON
using System.Xml.Serialization;  // Library for XML

namespace EasyLog
{
    public class LogData
    {

        public DateTime Timestamp { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public long Size { get; set; }
        public long TransferTime { get; set; }


        // Method to transform LogData into JSON
        public string ToJSON()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }

        // Method to transform LogData into XML
        public string ToXML()
        {
            var serializer = new XmlSerializer(typeof(LogData));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, this);
                return writer.ToString();
            }
        }
    }
}