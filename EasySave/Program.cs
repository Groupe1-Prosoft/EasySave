using System;
using System.Collections.Generic;
using EasySave.Models;
using EasySave.Services;
using EasyLog;
using EasySave.Localization;

namespace EasySave
{
    class Program
    {
        private static BackupService _backupService = new BackupService();
        private static LanguageManager _languageManager = new LanguageManager();

        static void Main(string[] args)
        {
            // Ligne de commande 
            if (args.Length > 0)
            {
                ExecuteFromCommandeLine(args[0]);
                return;
            }

            SelectLanguage();
            Console.Title = _languageManager.GetText("Title");

            bool keepRunning = true;

            while (keepRunning)
            {
                Console.Clear();
                ShowHeader();

                Console.WriteLine(_languageManager.GetText("SelectOption"));
                Console.WriteLine(_languageManager.GetText("ListJobs"));
                Console.WriteLine(_languageManager.GetText("CreateJob"));
                Console.WriteLine(_languageManager.GetText("ExecuteJob"));
                Console.WriteLine(_languageManager.GetText("ExecuteAllJobs"));
                Console.WriteLine(_languageManager.GetText("DeleteJob"));
                Console.WriteLine(_languageManager.GetText("Exit"));
                Console.WriteLine(_languageManager.GetText("Separator"));
                Console.Write(_languageManager.GetText("YourChoice"));

                string userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1": ListJobs(); break;
                    case "2": CreateJob(); break;
                    case "3": ExecuteJob(); break;
                    case "4": ExecuteAllJobs(); break;
                    case "5": DeleteJob(); break;
                    case "6": keepRunning = false; Console.WriteLine(_languageManager.GetText("Goodbye")); break;
                    default: DisplayMessage(_languageManager.GetText("InvalidOption"), ConsoleColor.Red); break;
                }
            }
        }

        static void ListJobs()
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine(_languageManager.GetText("ListHeader"));

            if (_backupService.Jobs.Count == 0)
            {
                Console.WriteLine(_languageManager.GetText("NoJobs"));
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
            Console.WriteLine(_languageManager.GetText("CreateHeader"));

            if (_backupService.Jobs.Count >= 5)
            {
                DisplayMessage(_languageManager.GetText("JobLimitReached"), ConsoleColor.Red);
                return;
            }

            Console.Write(_languageManager.GetText("EnterJobName"));
            string name = Console.ReadLine();
            Console.Write(_languageManager.GetText("EnterSourcePath"));
            string source = Console.ReadLine();
            Console.Write(_languageManager.GetText("EnterTargetPath"));
            string target = Console.ReadLine();
            Console.Write(_languageManager.GetText("SelectType"));
            string typeSelection = Console.ReadLine();
            BackupType type = (typeSelection == "2") ? BackupType.Differential : BackupType.Full;

            BackupJob newJob = new BackupJob(name, source, target, type);
            bool added = _backupService.AddJob(newJob);

            if (added) DisplayMessage(_languageManager.GetText("JobCreated"), ConsoleColor.Green);
            else DisplayMessage(_languageManager.GetText("JobLimitReached"), ConsoleColor.Red);
        }

        static void ExecuteJob()
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine(_languageManager.GetText("ExecuteHeader"));

            if (_backupService.Jobs.Count == 0)
            {
                Console.WriteLine(_languageManager.GetText("NoJobs"));
                WaitUser();
                return;
            }

            for (int i = 0; i < _backupService.Jobs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_backupService.Jobs[i].Name}");
            }

            Console.WriteLine();
            Console.Write(_languageManager.GetText("EnterJobNumber"));
            string input = Console.ReadLine();

            if (int.TryParse(input, out int jobNumber) && jobNumber >= 1 && jobNumber <= _backupService.Jobs.Count)
            {
                BackupJob jobToRun = _backupService.Jobs[jobNumber - 1];
                _backupService.ExecuteJob(jobToRun);
                DisplayMessage(_languageManager.GetText("BackupFinished"), ConsoleColor.Green);
            }
            else
            {
                DisplayMessage(_languageManager.GetText("JobNotFound"), ConsoleColor.Red);
            }
        }

        static void ExecuteAllJobs()
        {
            Console.Clear();
            ShowHeader();

            if (_backupService.Jobs.Count == 0)
            {
                Console.WriteLine(_languageManager.GetText("NoJobs"));
                WaitUser();
                return;
            }

            foreach (var job in _backupService.Jobs)
            {
                Console.WriteLine($"Executing: {job.Name}");
                _backupService.ExecuteJob(job);
            }

            DisplayMessage(_languageManager.GetText("BackupFinished"), ConsoleColor.Green);
        }

        static void DeleteJob()
        {
            Console.Clear();
            ShowHeader();
            Console.WriteLine(_languageManager.GetText("DeleteHeader"));

            if (_backupService.Jobs.Count == 0)
            {
                Console.WriteLine(_languageManager.GetText("NoJobs"));
                WaitUser();
                return;
            }

            for (int i = 0; i < _backupService.Jobs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_backupService.Jobs[i].Name}");
            }

            Console.WriteLine();
            Console.Write(_languageManager.GetText("EnterJobNumber"));
            string input = Console.ReadLine();

            if (int.TryParse(input, out int jobNumber))
            {
                bool deleted = _backupService.DeleteJob(jobNumber);
                if (deleted) DisplayMessage(_languageManager.GetText("JobDeleted"), ConsoleColor.Green);
                else DisplayMessage(_languageManager.GetText("JobNotFound"), ConsoleColor.Red);
            }
            else
            {
                DisplayMessage(_languageManager.GetText("InvalidOption"), ConsoleColor.Red);
            }
        }

        static void SelectLanguage()
        {
            Console.Clear();
            Console.WriteLine("EasySave 1.0");
            Console.WriteLine();
            Console.WriteLine("Select language: 1. English  2. Français");
            Console.Write("> ");
            string choice = Console.ReadLine();

            if (choice == "2") _languageManager.SetLanguage("fr");
            else _languageManager.SetLanguage("en");

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
            Console.WriteLine(_languageManager.GetText("PressEnterReturn"));
            Console.ReadLine();
        }

        static void ExecuteFromCommandeLine(string argument)
        {
            CommandLineService cmdService = new CommandLineService();
            List<int> jobIndices = cmdService.ParseArgument(argument);

            foreach (int index in jobIndices)
            {
                if (index >= 1 && index <= _backupService.Jobs.Count)
                {
                    BackupJob job = _backupService.Jobs[index - 1];
                    Console.WriteLine($"Executing job {index}: {job.Name}");
                    _backupService.ExecuteJob(job);
                    Console.WriteLine($"Job {index} completed.");
                }
                else
                {
                    Console.WriteLine($"Job {index} not found.");
                }
            }
        }
    }
}