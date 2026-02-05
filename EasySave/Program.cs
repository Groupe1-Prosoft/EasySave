using System;
using System.Collections.Generic;
using EasySave.Models;
using EasySave.Services;
using EasySave.Localization;
using EasySave.Views;
using System.IO;

namespace EasySave
{
    class Program
    {
        private static LanguageManager _languageManager = new LanguageManager();
        private static BackupService _backupService = new BackupService(_languageManager);
        private static ConsoleView _view = null!;

        static void Main(string[] args)
        {
            _view = new ConsoleView(_languageManager);

            if (args.Length > 0)
            {
                ExecuteFromCommandeLine(args[0]);
                return;
            }

            SelectLanguage();
            _view.SetTitle(_languageManager.GetText("Title"));

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
                    case "4": ExecuteAllJobs(); break;
                    case "5": DeleteJob(); break;
                    case "6":
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

        static void ListJobs()
        {
            _view.ClearAndShowHeader();
            _view.ShowText("ListHeader");
            _view.DisplayJobList(_backupService.Jobs);
            _view.WaitUser();
        }

        static void CreateJob()
        {
            _view.ClearAndShowHeader();
            _view.ShowText("CreateHeader");

            if (_backupService.Jobs.Count >= 5)
            {
                _view.DisplayError(_languageManager.GetText("JobLimitReached"));
                _view.WaitUser();
                return;
            }

            string name = _view.PromptInput("EnterJobName");
            if (string.IsNullOrWhiteSpace(name))
            {
                _view.DisplayError(_languageManager.GetText("InvalidOption"));
                _view.WaitUser();
                return;
            }

            string source = _view.PromptInput("EnterSourcePath");
            if (string.IsNullOrWhiteSpace(source) || !Directory.Exists(source))
            {
                _view.DisplayError(_languageManager.GetText("JobNotFound"));
                _view.WaitUser();
                return;
            }

            string target = _view.PromptInput("EnterTargetPath");
            if (string.IsNullOrWhiteSpace(target))
            {
                _view.DisplayError(_languageManager.GetText("InvalidOption"));
                _view.WaitUser();
                return;
            }

            string typeSelection = _view.PromptInput("SelectType");
            if (typeSelection != "1" && typeSelection != "2")
            {
                _view.DisplayError(_languageManager.GetText("InvalidOption"));
                _view.WaitUser();
                return;
            }
            BackupType type = (typeSelection == "2") ? BackupType.Differential : BackupType.Full;

            BackupJob newJob = new BackupJob(name, source, target, type);
            bool added = _backup_service.AddJob(newJob);

            if (added) _view.DisplaySuccess(_languageManager.GetText("JobCreated"));
            else _view.DisplayError(_language_manager.GetText("JobLimitReached"));
            _view.WaitUser();
        }

        static void ExecuteJob()
        {
            _view.ClearAndShowHeader();
            _view.ShowText("ExecuteHeader");

            if (_backupService.Jobs.Count == 0)
            {
                _view.ShowText("NoJobs");
                _view.WaitUser();
                return;
            }

            _view.DisplayJobSelection(_backupService.Jobs);
            string input = _view.PromptInput("EnterJobNumber");

            if (int.TryParse(input, out int jobNumber) && jobNumber >= 1 && jobNumber <= _backupService.Jobs.Count)
            {
                BackupJob jobToRun = _backupService.Jobs[jobNumber - 1];
                _backupService.ExecuteJob(jobToRun);
                _view.DisplaySuccess(_languageManager.GetText("BackupFinished"));
            }
            else
            {
                _view.DisplayError(_languageManager.GetText("JobNotFound"));
            }
            _view.WaitUser();
        }

        static void ExecuteAllJobs()
        {
            _view.ClearAndShowHeader();

            if (_backupService.Jobs.Count == 0)
            {
                _view.ShowText("NoJobs");
                _view.WaitUser();
                return;
            }

            foreach (var job in _backupService.Jobs)
            {
                _view.ShowTextWithParam("ExecutingJob", job.Name);
                _backupService.ExecuteJob(job);
            }

            _view.DisplaySuccess(_languageManager.GetText("BackupFinished"));
            _view.WaitUser();
        }

        static void DeleteJob()
        {
            _view.ClearAndShowHeader();
            _view.ShowText("DeleteHeader");

            if (_backup_service.Jobs.Count == 0)
            {
                _view.ShowText("NoJobs");
                _view.WaitUser();
                return;
            }

            _view.DisplayJobSelection(_backupService.Jobs);
            string input = _view.PromptInput("EnterJobNumber");

            if (int.TryParse(input, out int jobNumber))
            {
                bool deleted = _backupService.DeleteJob(jobNumber);
                if (deleted) _view.DisplaySuccess(_languageManager.GetText("JobDeleted"));
                else _view.DisplayError(_languageManager.GetText("JobNotFound"));
            }
            else
            {
                _view.DisplayError(_language_manager.GetText("InvalidOption"));
            }
            _view.WaitUser();
        }

        static void SelectLanguage()
        {
            _view.ShowLanguageSelection();
            string choice = _view.GetInput();

            if (choice == "2") _languageManager.SetLanguage("fr");
            else _languageManager.SetLanguage("en");

            System.Threading.Thread.Sleep(400);
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
                    _view.ShowTextWithParam("ExecutingJob", job.Name);
                    _backupService.ExecuteJob(job);
                    _view.ShowTextWithParam("JobExecuted", index);
                }
                else
                {
                    _view.ShowText("JobNotFound");
                }
            }
        }
    }
}