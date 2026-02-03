using System;

namespace EasyLog
{
    public class Logger
    {
        // Constructeur qui ne fait rien (juste pour que ça compile)
        public Logger(string path)
        {
            // On fait semblant d'initialiser le dossier
            Console.WriteLine($"[DEBUG] Logger initialisé vers : {path}");
        }

        // Méthode qui simule l'écriture
        public void WriteLog(string jobName, string source, string target, long size, long time)
        {
            // Au lieu d'écrire dans un fichier JSON (boulot de ton collègue),
            // on écrit juste dans la console pour te prouver que ton BackupService a bien appelé le logger.

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"   -> [LOG APPELÉ] Job: {jobName} | Fichier: {source} | Temps: {time}ms");
            Console.ResetColor();
        }
    }
}