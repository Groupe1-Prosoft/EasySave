using System;
using EasyLog; // Ça ne doit plus être souligné en rouge

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- TEST FINAL DU LOGGER ---");

        // 1. Création d'une fausse donnée (Simulation)
        var testLog = new LogData
        {
            Name = "Sauvegarde_Test_Validation",
            Source = @"C:\Projet\Source",
            Target = @"D:\Backup\Destination",
            Size = 4500,           // 4.5 Ko
            TransferTime = 150,    // 150 ms
            Timestamp = DateTime.Now
        };

        // 2. Initialisation de ton Logger
        Logger logger = new Logger();

        // 3. Écriture
        Console.Write("Tentative d'écriture... ");
        bool reussite = logger.WriteLog(testLog);

        // 4. Vérification
        if (reussite)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SUCCÈS !");
            Console.ResetColor();
            Console.WriteLine($"Le fichier JSON a été généré ici :");
            Console.WriteLine(logger.CreateDailyLogFile());
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ÉCHEC (Erreur d'écriture).");
        }

        Console.WriteLine("\nAppuie sur Entrée pour fermer.");
        Console.ReadLine();
    }
}