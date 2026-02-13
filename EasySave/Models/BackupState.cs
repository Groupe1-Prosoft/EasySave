using System;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

namespace EasySave.Models
{
    /// <summary>
    /// Represents runtime progress data stored in state files.
    /// </summary>
    public class BackupState
    {
        /// <summary>
        /// Gets or sets the backup job name.
        /// </summary>
        public string? JobName { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the last state update.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the job state (e.g., ACTIF, NON ACTIF).
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the total number of eligible files.
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Gets or sets the total size in bytes.
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// Gets or sets the remaining files count.
        /// </summary>
        public int FilesRemaining { get; set; }

        /// <summary>
        /// Gets or sets the remaining size in bytes.
        /// </summary>
        public long SizeRemaining { get; set; }

        /// <summary>
        /// Gets or sets the progression percentage.
        /// </summary>
        public int Progression { get; set; }

        /// <summary>
        /// Gets or sets the current source file being processed.
        /// </summary>
        public string? CurrentSourceFile { get; set; }

        /// <summary>
        /// Gets or sets the current target file being processed.
        /// </summary>
        public string? CurrentTargetFile { get; set; }

        /// <summary>
        /// Writes state.json.
        /// </summary>
        public bool UpdateStateJSON()
        {
            return WriteStateFile("state.json", ToJSON());
        }

        /// <summary>
        /// Writes state.xml.
        /// </summary>
        public bool UpdateStateXML()
        {
            return WriteStateFile("state.xml", ToXML());
        }

        /// <summary>
        /// Serializes the state to JSON.
        /// </summary>
        public string ToJSON()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }

        /// <summary>
        /// Serializes the state to XML.
        /// </summary>
        public string ToXML()
        {
            var serializer = new XmlSerializer(typeof(BackupState));
            using var writer = new StringWriter();
            serializer.Serialize(writer, this);
            return writer.ToString();
        }

        private static bool WriteStateFile(string fileName, string content)
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string path = Path.Combine(appData, "EasySave", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Initializes a default inactive state.
        /// </summary>
        public BackupState()
        {
            Timestamp = DateTime.Now;
            State = "NON ACTIF";
            JobName = "";
            CurrentSourceFile = "";
            CurrentTargetFile = "";
        }
    }
}