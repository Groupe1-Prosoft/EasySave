using System;
<<<<<<< HEAD
using System.Collections.Generic;
using EasyLog; // INDISPENSABLE : Importe ta DLL (Model + Service)

namespace EasySave
{
    class Program
    {
        // --- PRINCIPE SOLID (DIP) ---
        // On dépend de l'Abstraction (Interface), pas de la Concrétion (Classe).
        // C'est ça qui rend ton code "Pro" et modulaire.
        private static readonly ILogger _logger = new Logger();

        // --- GESTION DES LANGUES (VUE) ---
        enum Language { English, French }
        static Language _lang = Language.English;

        static readonly Dictionary<string, string> _en = new()
        {
            ["Title"] = "EasySave 1.0",
            ["SelectLanguage"] = "Select language: 1. English  2. Français",
            ["InvalidLanguage"] = "Invalid selection. Defaulting to English.",
            ["SelectOption"] = "Select an option:",
            ["ListJobs"] = "1. List backup jobs",
            ["CreateJob"] = "2. Create a backup job",
            ["ExecuteJob"] = "3. Execute a backup job",
            ["Exit"] = "4. Exit",
            ["Separator"] = "-----------------------------------",
            ["YourChoice"] = "Your choice: ",
            ["InvalidOption"] = "Invalid option. Please try again.",
            ["Goodbye"] = "Goodbye!",
            ["ListHeader"] = "--- List of Backup Jobs ---",
            ["NoJobs"] = "No jobs configured yet.",
            ["CreateHeader"] = "--- Create a New Backup Job ---",
            ["EnterJobName"] = "Enter Job Name: ",
            ["EnterSourcePath"] = "Enter Source Path: ",
            ["EnterTargetPath"] = "Enter Target Path: ",
            ["SelectType"] = "Select Type (1. Full / 2. Differential): ",
            ["JobCreated"] = "Job created successfully !",
            ["ExecuteHeader"] = "--- Execute a Backup Job ---",
            ["EnterJobNumber"] = "Enter job number to execute: ",
            ["ExecutionInProgress"] = "Execution in progress (Simulated)...",
            ["BackupFinished"] = "Backup finished successfully!",
            ["LogSuccess"] = "Log file updated: ",
            ["LogError"] = "Error writing log file.",
            ["PressEnterReturn"] = "Press Enter to return to menu..."
        };

        static readonly Dictionary<string, string> _fr = new()
        {
            ["Title"] = "EasySave 1.0",
            ["SelectLanguage"] = "Choisissez la langue : 1. Anglais  2. Français",
            ["InvalidLanguage"] = "Sélection invalide. Anglais choisi par défaut.",
            ["SelectOption"] = "Sélectionnez une option :",
            ["ListJobs"] = "1. Lister les tâches de sauvegarde",
            ["CreateJob"] = "2. Créer une tâche de sauvegarde",
            ["ExecuteJob"] = "3. Exécuter une tâche de sauvegarde",
            ["Exit"] = "4. Quitter",
            ["Separator"] = "-----------------------------------",
            ["YourChoice"] = "Votre choix : ",
            ["InvalidOption"] = "Option invalide. Veuillez réessayer.",
            ["Goodbye"] = "Au revoir !",
            ["ListHeader"] = "--- Liste des tâches de sauvegarde ---",
            ["NoJobs"] = "Aucune tâche configurée pour l'instant.",
            ["CreateHeader"] = "--- Créer une nouvelle tâche de sauvegarde ---",
            ["EnterJobName"] = "Entrez le nom de la tâche : ",
            ["EnterSourcePath"] = "Entrez le chemin source : ",
            ["EnterTargetPath"] = "Entrez le chemin cible : ",
            ["SelectType"] = "Sélectionnez le type (1. Complète / 2. Différentielle) : ",
            ["JobCreated"] = "Tâche créée avec succès !",
            ["ExecuteHeader"] = "--- Exécuter une tâche de sauvegarde ---",
            ["EnterJobNumber"] = "Entrez le numéro de la tâche à exécuter : ",
            ["ExecutionInProgress"] = "Exécution en cours (Simulation)...",
            ["BackupFinished"] = "Sauvegarde terminée avec succès !",
            ["LogSuccess"] = "Fichier de log mis à jour : ",
            ["LogError"] = "Erreur lors de l'écriture du log.",
            ["PressEnterReturn"] = "Appuyez sur Entrée pour revenir au menu..."
        };

        static string L(string key) => _lang == Language.English ? _en[key] : _fr[key];

        static void Main(string[] args)
        {
            SelectLanguage();
            Console.Title = L("Title");

            bool keepRunning = true;

            while (keepRunning)
            {
                Console.Clear();
                ShowHeader();

                Console.WriteLine(L("SelectOption"));
                Console.WriteLine(L("ListJobs"));
                Console.WriteLine(L("CreateJob"));
                Console.WriteLine(L("ExecuteJob"));
                Console.WriteLine(L("Exit"));
                Console.WriteLine(L("Separator"));
                Console.Write(L("YourChoice"));

                string userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1":
                        ListJobs();
                        break;
                    case "2":
                        CreateJob();
                        break;
                    case "3":
                        ExecuteJob(); // C'est ici que la magie opère !
                        break;
                    case "4":
                        keepRunning = false;
                        Console.WriteLine(L("Goodbye"));
                        break;
                    default:
                        DisplayMessage(L("InvalidOption"), ConsoleColor.Red);
                        break;
                }
            }
        }

        static void SelectLanguage()
        {
            Console.Clear();
            Console.WriteLine(_en["Title"]);
            Console.WriteLine();
            Console.WriteLine(_en["SelectLanguage"]);
            Console.Write("> ");
            string choice = Console.ReadLine();

            if (choice == "2") _lang = Language.French;
            else if (choice == "1") _lang = Language.English;
            else
            {
                Console.WriteLine(_en["InvalidLanguage"]);
                _lang = Language.English;
            }
            System.Threading.Thread.Sleep(400);
        }

        static void ShowHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ______                    ____                  ");
            Console.WriteLine(" |  ____|                  / ___|   __ ___    ___  ");
            Console.WriteLine(" | |__   __ _ ___ _   _    \\___ \\ / _` \\ \\ / / _ \\");
            Console.WriteLine(" |  __| / _` / __| | | |    ___) | (_| |\\ V /  __/");
            Console.WriteLine(" | |___| (_| \\__ \\ |_| |   |____/ \\__,_| \\_/ \\___|");
            Console.WriteLine(" |______|\\__,_|___/\\__, |                          ");
            Console.WriteLine("                       |___/                           ");
            Console.WriteLine("---------------------------------------");
            Console.ResetColor();
        }

        static void ListJobs()
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine(L("ListHeader"));
            Console.WriteLine(L("NoJobs")); // Pour l'instant on n'a pas la liste réelle
            WaitUser();
        }

        static void CreateJob()
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine(L("CreateHeader"));

            Console.Write(L("EnterJobName"));
            string name = Console.ReadLine(); // On capture mais on ne stocke pas encore

            Console.Write(L("EnterSourcePath"));
            Console.ReadLine();

            Console.Write(L("EnterTargetPath"));
            Console.ReadLine();

            Console.Write(L("SelectType"));
            Console.ReadLine();

            DisplayMessage(L("JobCreated"), ConsoleColor.Green);
        }

        // --- C'EST ICI QUE TU UTILISES TA DLL (MVVM : Le ViewModel appelle le Modèle) ---
        static void ExecuteJob()
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine(L("ExecuteHeader"));

            Console.Write(L("EnterJobNumber"));
            string jobId = Console.ReadLine(); // Simulation du choix

            Console.WriteLine(L("ExecutionInProgress"));

            // Simulation d'un travail (barre de progression fictive)
            System.Threading.Thread.Sleep(1000);

            // 1. CRÉATION DU MODÈLE (LogData)
            // On simule des données comme si le travail venait de se faire
            var logData = new LogData
            {
                Name = $"Job_Numero_{jobId}",
                Source = @"C:\Utilisateurs\Documents",
                Target = @"D:\Sauvegardes\Documents",
                Size = 125000,          // 125 Ko
                TransferTime = 540,     // 540 ms
                Timestamp = DateTime.Now
            };

            // 2. APPEL DU SERVICE VIA L'INTERFACE (Logger)
            bool success = _logger.WriteLog(logData);

            if (success)
            {
                Console.WriteLine(L("BackupFinished"));
                // Petite astuce pour afficher le chemin du fichier (on triche un peu en castant pour l'affichage)
                if (_logger is Logger concreteLogger)
                {
                    Console.WriteLine($"{L("LogSuccess")} {concreteLogger.CreateDailyLogFile()}");
                }
                DisplayMessage("", ConsoleColor.Green);
            }
            else
            {
                DisplayMessage(L("LogError"), ConsoleColor.Red);
            }
        }

        static void DisplayMessage(string message, ConsoleColor color)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ResetColor();
            }
            WaitUser();
        }

        static void WaitUser()
        {
            Console.WriteLine();
            Console.WriteLine(L("PressEnterReturn"));
            Console.ReadLine();
        }
=======
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
>>>>>>> EasyLog-DLL
    }
}