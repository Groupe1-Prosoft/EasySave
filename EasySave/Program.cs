using System;
using System.Collections.Generic;
using EasySave.Models;
using EasySave.Services;
using EasySave.Localization;
using EasySave.Views;

namespace EasySave
{
    /// <summary>
    /// Console entry point orchestrating UI flow and service calls.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Provides UI localization.
        /// </summary>
        private static LanguageManager _languageManager = new LanguageManager();

        /// <summary>
        /// Stores runtime configuration values.
        /// </summary>
        private static Configuration _configuration = new Configuration();

        /// <summary>
        /// Provides job management and backup execution.
        /// </summary>
        private static BackupService _backupService = null!;

        /// <summary>
        /// Provides console rendering utilities.
        /// </summary>
        private static ConsoleView _view = null!;

        /// <summary>
        /// Application entry point.
        /// </summary>
        static void Main(string[] args)
        {
            _view = new ConsoleView(_languageManager);
            _configuration.LoadConfig();
            _backupService = new BackupService(_configuration);

            if (args.Length > 0)
            {
                CommandLineService cmd = new CommandLineService();
                List<int> ids = cmd.ParseArgument(args[0]);
                _backupService.ExecuteSequential(ids);
                return;
            }

            SelectLanguage();
            _configuration.LogFormat = _view.SelectLogFormat();
            _configuration.SaveConfig();
            _backupService = new BackupService(_configuration);

            bool keepRunning = true;

            while (keepRunning)
            {
                _view.ShowMenu();
                string userChoice = _view.GetInput();

                switch (userChoice)
                {
                    case "1": ListJobs(); break;
                    case "2": CreateJob(); break;
                    case "3": ExecuteJob(); break;
                    case "4": ExecuteSequential(); break;
                    case "5": DeleteJob(); break;
                    case "6":
                        _configuration.LogFormat = _view.SelectLogFormat();
                        _configuration.SaveConfig();
                        _backupService = new BackupService(_configuration);
                        break;
                    case "7":
                        keepRunning = false;
                        _view.ShowText("Goodbye");
                        break;
                    default:
                        _view.DisplayError(_languageManager.GetText("InvalidOption"));
                        _view.WaitUser();
                        break;
                }
            }
        }

        /// <summary>
        /// Displays configured jobs and returns to the menu.
        /// </summary>
        static void ListJobs()
        {
            _view.ClearAndShowHeader();
            _view.ShowText("ListHeader");
            _view.DisplayJobList(_configuration.GetJobs());
            _view.WaitUser();
        }

        /// <summary>
        /// Creates a job after validating user input.
        /// </summary>
        static void CreateJob()
        {
            _view.ClearAndShowHeader();
            _view.ShowText("CreateHeader");

            string name = _view.PromptInput("EnterJobName");
            string source = _view.PromptInput("EnterSourcePath");
            string target = _view.PromptInput("EnterTargetPath");
            string typeSelection = _view.PromptInput("SelectType");

            BackupType type = typeSelection == "2" ? BackupType.Differential : BackupType.Full;

            var job = new BackupJob
            {
                Name = name,
                SourceDir = source,
                TargetDir = target,
                Type = type
            };

            if (!_configuration.AddJob(job))
                _view.DisplayError(_languageManager.GetText("JobLimitReached"));
            else
                _view.DisplaySuccess(_languageManager.GetText("JobCreated"));

            _view.WaitUser();
        }

        /// <summary>
        /// Executes a single job chosen by the user.
        /// </summary>
        static void ExecuteJob()
        {
            _view.ClearAndShowHeader();
            _view.ShowText("ExecuteHeader");

            var jobs = _configuration.GetJobs();
            _view.DisplayJobSelection(jobs);

            string input = _view.PromptInput("EnterJobNumber");
            if (int.TryParse(input, out int id))
            {
                var job = jobs.Find(j => j.Id == id);
                if (job != null && _backupService.ExecuteJob(job))
                    _view.DisplaySuccess(_languageManager.GetText("BackupFinished"));
                else
                    _view.DisplayError(_languageManager.GetText("JobNotFound"));
            }
            else
            {
                _view.DisplayError(_languageManager.GetText("InvalidOption"));
            }

            _view.WaitUser();
        }

        /// <summary>
        /// Executes all configured jobs sequentially.
        /// </summary>
        static void ExecuteSequential()
        {
            _view.ClearAndShowHeader();

            var jobs = _configuration.GetJobs();
            if (jobs.Count == 0)
            {
                _view.ShowText("NoJobs");
                _view.WaitUser();
                return;
            }

            List<int> ids = new List<int>();
            foreach (var job in jobs) ids.Add(job.Id);

            _backupService.ExecuteSequential(ids);
            _view.DisplaySuccess(_languageManager.GetText("BackupFinished"));
            _view.WaitUser();
        }

        /// <summary>
        /// Deletes a job selected by the user.
        /// </summary>
        static void DeleteJob()
        {
            _view.ClearAndShowHeader();
            _view.ShowText("DeleteHeader");

            var jobs = _configuration.GetJobs();
            _view.DisplayJobSelection(jobs);

            string input = _view.PromptInput("EnterJobNumber");
            if (int.TryParse(input, out int id))
            {
                if (_configuration.RemoveJob(id))
                    _view.DisplaySuccess(_languageManager.GetText("JobDeleted"));
                else
                    _view.DisplayError(_languageManager.GetText("JobNotFound"));
            }
            else
            {
                _view.DisplayError(_languageManager.GetText("InvalidOption"));
            }

            _view.WaitUser();
        }

        /// <summary>
        /// Prompts the user to choose the UI language.
        /// </summary>
        static void SelectLanguage()
        {
            _view.ShowLanguageSelection();
            string choice = _view.GetInput();
            _languageManager.SetLanguage(choice == "2" ? "fr" : "en");
        }
    }
}