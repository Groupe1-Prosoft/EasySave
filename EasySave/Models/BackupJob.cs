namespace EasySave.Models
{
    public class BackupJob
    {
        // Le ? rend la variable "nullable" (elle a le droit d'être vide)
        public string? Name { get; set; }
        public string? SourceDirectory { get; set; }
        public string? TargetDirectory { get; set; }
        public BackupType Type { get; set; }

        // Constructeur vide (nécessaire pour plus tard)
        public BackupJob() { }

        // Constructeur complet
        public BackupJob(string name, string source, string target, BackupType type)
        {
            Name = name;
            SourceDirectory = source;
            TargetDirectory = target;
            Type = type;
        }
    }
}