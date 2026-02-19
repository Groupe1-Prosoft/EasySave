using System;
using System.Collections.Generic;
using EasySave.Models;
using EasySave.Views;
using EasySave.Localization;

namespace EasySave.Services
{
    /// <summary>
    /// Handles all interactive menu operations.
    /// Each menu action is a dedicated method (high granularity, single responsibility).
    /// Goal: Extract 200 lines from Program.Main() and make each action testable and reusable.
    /// </summary>
    public class MenuController
    {
        private readonly ServiceContainer _services;
        private readonly BackupService _backupService;
        private readonly Configuration _configuration;
        private readonly ConsoleView _view;
        private readonly LanguageManager _languageManager;

        /// <summary>
        /// Initializes the menu controller with all required services.
        /// </summary>
        public MenuController(ServiceContainer services)
        {
            _services = services;
            _backupService = services.GetBackupService();
            _configuration = services.GetConfiguration();
            _view = services.GetConsoleView();
            _languageManager = services.GetLanguageManager();
        }

        /// <summary>
        /// Runs the main menu loop until user exits.
        /// </summary>
        public void Run()
        {
            SetupLanguageAndFormat();

            bool keepRunning = true;
            while (keepRunning)
            {
                _view.ShowMenu();
                string userChoice = _view.GetInput();

                keepRunning = HandleMenuOption(userChoice);
            }

            Console.WriteLine(_languageManager.GetText("Goodbye"));
        }

        /// <summary>
        /// Sets up language selection and log format at startup.
        /// </summary>
        private void SetupLanguageAndFormat()
        {
            Console.Clear();
            Console.WriteLine("EasySave 1.0");
            Console.WriteLine();
            Console.WriteLine("Select language: 1. English  2. Français");
            Console.Write("> ");
            string languageChoice = Console.ReadLine() ?? string.Empty;
            _languageManager.SetLanguage(languageChoice == "2" ? "fr" : "en");

            _configuration.LogFormat = _view.SelectLogFormat();
            _configuration.SaveConfig();
            _services.RefreshBackupService();
        }

        /// <summary>
        /// Dispatches menu choice to appropriate handler.
        /// Returns false to exit menu loop.
        /// </summary>
        private bool HandleMenuOption(string choice)
        {
            return choice switch
            {
                "1" => HandleListJobs(),
                "2" => HandleCreateJob(),
                "3" => HandleExecuteJob(),
                "4" => HandleExecuteAllJobs(),
                "5" => HandleDeleteJob(),
                "6" => HandleChangeLogFormat(),
                "7" => false, // Exit
                _ => HandleInvalidOption(),
            };
        }

        /// <summary>
        /// Action 1: List all backup jobs.
        /// </summary>
        private bool HandleListJobs()
        {
            Console.Clear();
            Console.WriteLine(_languageManager.GetText("ListHeader"));

            var jobsList = _configuration.GetJobs();
            if (jobsList.Count == 0)
            {
                Console.WriteLine(_languageManager.GetText("NoJobs"));
            }
            else
            {
                for (int i = 0; i < jobsList.Count; i++)
                {
                    var job = jobsList[i];
                    Console.WriteLine($"{i + 1}. {job.Name} | {job.Type} | {job.SourceDir} -> {job.TargetDir}");
                }
            }

            Console.WriteLine();
            Console.WriteLine(_languageManager.GetText("PressEnterReturn"));
            Console.ReadLine();
            return true;
        }

        /// <summary>
        /// Action 2: Create a new backup job.
        /// </summary>
        private bool HandleCreateJob()
        {
            Console.Clear();
            Console.WriteLine(_languageManager.GetText("CreateHeader"));

            Console.Write(_languageManager.GetText("EnterJobName"));
            string name = Console.ReadLine() ?? string.Empty;

            Console.Write(_languageManager.GetText("EnterSourcePath"));
            string source = Console.ReadLine() ?? string.Empty;

            Console.Write(_languageManager.GetText("EnterTargetPath"));
            string target = Console.ReadLine() ?? string.Empty;

            Console.Write(_languageManager.GetText("SelectType"));
            string typeSelection = Console.ReadLine() ?? string.Empty;

            BackupType type = typeSelection == "2" ? BackupType.Differential : BackupType.Full;

            var newJob = new BackupJob
            {
                Name = name,
                SourceDir = source,
                TargetDir = target,
                Type = type
            };

            if (!_configuration.AddJob(newJob))
            {
                _view.DisplayError(_languageManager.GetText("JobLimitReached"));
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(_languageManager.GetText("JobCreated"));
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine(_languageManager.GetText("PressEnterReturn"));
            Console.ReadLine();
            return true;
        }

        /// <summary>
        /// Action 3: Execute a single backup job.
        /// </summary>
        private bool HandleExecuteJob()
        {
            Console.Clear();
            Console.WriteLine(_languageManager.GetText("ExecuteHeader"));

            var jobsExec = _configuration.GetJobs();
            for (int i = 0; i < jobsExec.Count; i++)
            {
                Console.WriteLine($"{jobsExec[i].Id}. {jobsExec[i].Name}");
            }

            Console.Write(_languageManager.GetText("EnterJobNumber"));
            string inputExec = Console.ReadLine() ?? string.Empty;

            if (int.TryParse(inputExec, out int execId))
            {
                var job = jobsExec.Find(j => j.Id == execId);
                if (job != null && _backupService.ExecuteJob(job))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(_languageManager.GetText("BackupFinished"));
                    Console.ResetColor();
                }
                else
                {
                    _view.DisplayError(_languageManager.GetText("JobNotFound"));
                }
            }
            else
            {
                _view.DisplayError(_languageManager.GetText("InvalidOption"));
            }

            Console.WriteLine();
            Console.WriteLine(_languageManager.GetText("PressEnterReturn"));
            Console.ReadLine();
            return true;
        }

        /// <summary>
        /// Action 4: Execute all backup jobs sequentially.
        /// </summary>
        private bool HandleExecuteAllJobs()
        {
            Console.Clear();

            var jobsSeq = _configuration.GetJobs();
            if (jobsSeq.Count == 0)
            {
                Console.WriteLine(_languageManager.GetText("NoJobs"));
                Console.WriteLine();
                Console.WriteLine(_languageManager.GetText("PressEnterReturn"));
                Console.ReadLine();
                return true;
            }

            List<int> ids = new List<int>();
            foreach (var job in jobsSeq) ids.Add(job.Id);

            _backupService.ExecuteSequential(ids);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(_languageManager.GetText("BackupFinished"));
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine(_languageManager.GetText("PressEnterReturn"));
            Console.ReadLine();
            return true;
        }

        /// <summary>
        /// Action 5: Delete a backup job.
        /// </summary>
        private bool HandleDeleteJob()
        {
            Console.Clear();
            Console.WriteLine(_languageManager.GetText("DeleteHeader"));

            var jobsDelete = _configuration.GetJobs();
            for (int i = 0; i < jobsDelete.Count; i++)
            {
                Console.WriteLine($"{jobsDelete[i].Id}. {jobsDelete[i].Name}");
            }

            Console.Write(_languageManager.GetText("EnterJobNumber"));
            string inputDelete = Console.ReadLine() ?? string.Empty;

            if (int.TryParse(inputDelete, out int deleteId))
            {
                if (_configuration.RemoveJob(deleteId))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(_languageManager.GetText("JobDeleted"));
                    Console.ResetColor();
                }
                else
                {
                    _view.DisplayError(_languageManager.GetText("JobNotFound"));
                }
            }
            else
            {
                _view.DisplayError(_languageManager.GetText("InvalidOption"));
            }

            Console.WriteLine();
            Console.WriteLine(_languageManager.GetText("PressEnterReturn"));
            Console.ReadLine();
            return true;
        }

        /// <summary>
        /// Action 6: Change log format (JSON/XML) and refresh services.
        /// </summary>
        private bool HandleChangeLogFormat()
        {
            _configuration.LogFormat = _view.SelectLogFormat();
            _configuration.SaveConfig();
            _services.RefreshBackupService();
            return true;
        }

        /// <summary>
        /// Handles invalid menu option selection.
        /// </summary>
        private bool HandleInvalidOption()
        {
            _view.DisplayError(_languageManager.GetText("InvalidOption"));
            return true;
        }
    }
}