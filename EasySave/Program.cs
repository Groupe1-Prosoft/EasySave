using System;
using System.Collections.Generic;
using EasySave.Models;
using EasySave.Services;
using EasyLog;

namespace EasySave
{
    class Program
    {
        private static BackupService _backupService = new BackupService();

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
            ["JobLimitReached"] = "Error: You cannot have more than 5 backup jobs.",
            ["ExecuteHeader"] = "--- Execute a Backup Job ---",
            ["EnterJobNumber"] = "Enter job number to execute (1-5): ",
            ["JobNotFound"] = "Job not found.",
            ["BackupFinished"] = "Backup finished!",
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
            ["JobLimitReached"] = "Erreur : Vous ne pouvez pas avoir plus de 5 tâches de sauvegarde.",
            ["ExecuteHeader"] = "--- Exécuter une tâche de sauvegarde ---",
            ["EnterJobNumber"] = "Entrez le numéro de la tâche à exécuter (1-5) : ",
            ["JobNotFound"] = "Tâche non trouvée.",
            ["BackupFinished"] = "Sauvegarde terminée !",
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
                    case "1": ListJobs(); break;
                    case "2": CreateJob(); break;
                    case "3": ExecuteJob(); break;
                    case "4": keepRunning = false; Console.WriteLine(L("Goodbye")); break;
                    default: DisplayMessage(L("InvalidOption"), ConsoleColor.Red); break;
                }
            }
        }

        static void ListJobs()
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine(L("ListHeader"));

            if (_backupService.Jobs.Count == 0)
            {
                Console.WriteLine(L("NoJobs"));
            }
            else
            {
                for (int i = 0; i < _backupService.Jobs.Count; i++)
                {
                    var job = _backupService.Jobs[i];
                    Console.WriteLine($"{i + 1}. {job.Name} | {job.Type} | {job.SourceDirectory} -> {job.TargetDirectory}");
                }
            }
            WaitUser();
        }

        static void CreateJob()
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine(L("CreateHeader"));

            if (_backupService.Jobs.Count >= 5)
            {
                DisplayMessage(L("JobLimitReached"), ConsoleColor.Red);
                return;
            }

            Console.Write(L("EnterJobName"));
            string name = Console.ReadLine();
            Console.Write(L("EnterSourcePath"));
            string source = Console.ReadLine();
            Console.Write(L("EnterTargetPath"));
            string target = Console.ReadLine();
            Console.Write(L("SelectType"));
            string typeSelection = Console.ReadLine();
            BackupType type = (typeSelection == "2") ? BackupType.Differential : BackupType.Full;

            BackupJob newJob = new BackupJob(name, source, target, type);
            bool added = _backupService.AddJob(newJob);

            if (added) DisplayMessage(L("JobCreated"), ConsoleColor.Green);
            else DisplayMessage(L("JobLimitReached"), ConsoleColor.Red);
        }

        static void ExecuteJob()
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine(L("ExecuteHeader"));

            if (_backupService.Jobs.Count == 0)
            {
                Console.WriteLine(L("NoJobs"));
                WaitUser();
                return;
            }

            for (int i = 0; i < _backupService.Jobs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_backupService.Jobs[i].Name}");
            }

            Console.WriteLine();
            Console.Write(L("EnterJobNumber"));
            string input = Console.ReadLine();

            if (int.TryParse(input, out int jobNumber) && jobNumber >= 1 && jobNumber <= _backupService.Jobs.Count)
            {
                BackupJob jobToRun = _backupService.Jobs[jobNumber - 1];
                _backupService.ExecuteJob(jobToRun);
                DisplayMessage(L("BackupFinished"), ConsoleColor.Green);
            }
            else
            {
                DisplayMessage(L("JobNotFound"), ConsoleColor.Red);
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
            else { Console.WriteLine(_en["InvalidLanguage"]); _lang = Language.English; }
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
    }
}