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
        /// Application entry point.
        /// </summary>
        public static void Main(string[] args)
        {
            var languageManager = LanguageManager.Instance;
            var configuration = new Configuration();
            configuration.LoadConfig();

            var view = new ConsoleView(languageManager);
            var backupService = new BackupService(configuration);

            if (args.Length > 0)
            {
                var cmd = new CommandLineService();
                List<int> ids = cmd.ParseArgument(args[0]);
                backupService.ExecuteSequential(ids);
                return;
            }

            Console.Clear();
            Console.WriteLine("EasySave 1.0");
            Console.WriteLine();
            Console.WriteLine("Select language: 1. English  2. Français");
            Console.Write("> ");
            string languageChoice = Console.ReadLine() ?? string.Empty;
            languageManager.SetLanguage(languageChoice == "2" ? "fr" : "en");

            configuration.LogFormat = view.SelectLogFormat();
            configuration.SaveConfig();
            backupService = new BackupService(configuration);

            bool keepRunning = true;

            while (keepRunning)
            {
                view.ShowMenu();
                string userChoice = view.GetInput();

                switch (userChoice)
                {
                    case "1":
                        Console.Clear();
                        Console.WriteLine(languageManager.GetText("ListHeader"));

                        var jobsList = configuration.GetJobs();
                        if (jobsList.Count == 0)
                        {
                            Console.WriteLine(languageManager.GetText("NoJobs"));
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
                        Console.WriteLine(languageManager.GetText("PressEnterReturn"));
                        Console.ReadLine();
                        break;

                    case "2":
                        Console.Clear();
                        Console.WriteLine(languageManager.GetText("CreateHeader"));

                        Console.Write(languageManager.GetText("EnterJobName"));
                        string name = Console.ReadLine() ?? string.Empty;

                        Console.Write(languageManager.GetText("EnterSourcePath"));
                        string source = Console.ReadLine() ?? string.Empty;

                        Console.Write(languageManager.GetText("EnterTargetPath"));
                        string target = Console.ReadLine() ?? string.Empty;

                        Console.Write(languageManager.GetText("SelectType"));
                        string typeSelection = Console.ReadLine() ?? string.Empty;

                        BackupType type = typeSelection == "2" ? BackupType.Differential : BackupType.Full;

                        var newJob = new BackupJob
                        {
                            Name = name,
                            SourceDir = source,
                            TargetDir = target,
                            Type = type
                        };

                        if (!configuration.AddJob(newJob))
                        {
                            view.DisplayError(languageManager.GetText("JobLimitReached"));
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(languageManager.GetText("JobCreated"));
                            Console.ResetColor();
                        }

                        Console.WriteLine();
                        Console.WriteLine(languageManager.GetText("PressEnterReturn"));
                        Console.ReadLine();
                        break;

                    case "3":
                        Console.Clear();
                        Console.WriteLine(languageManager.GetText("ExecuteHeader"));

                        var jobsExec = configuration.GetJobs();
                        for (int i = 0; i < jobsExec.Count; i++)
                        {
                            Console.WriteLine($"{jobsExec[i].Id}. {jobsExec[i].Name}");
                        }

                        Console.Write(languageManager.GetText("EnterJobNumber"));
                        string inputExec = Console.ReadLine() ?? string.Empty;

                        if (int.TryParse(inputExec, out int execId))
                        {
                            var job = jobsExec.Find(j => j.Id == execId);
                            if (job != null && backupService.ExecuteJob(job))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(languageManager.GetText("BackupFinished"));
                                Console.ResetColor();
                            }
                            else
                            {
                                view.DisplayError(languageManager.GetText("JobNotFound"));
                            }
                        }
                        else
                        {
                            view.DisplayError(languageManager.GetText("InvalidOption"));
                        }

                        Console.WriteLine();
                        Console.WriteLine(languageManager.GetText("PressEnterReturn"));
                        Console.ReadLine();
                        break;

                    case "4":
                        Console.Clear();

                        var jobsSeq = configuration.GetJobs();
                        if (jobsSeq.Count == 0)
                        {
                            Console.WriteLine(languageManager.GetText("NoJobs"));
                            Console.WriteLine();
                            Console.WriteLine(languageManager.GetText("PressEnterReturn"));
                            Console.ReadLine();
                            break;
                        }

                        List<int> ids = new List<int>();
                        foreach (var job in jobsSeq) ids.Add(job.Id);

                        backupService.ExecuteSequential(ids);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(languageManager.GetText("BackupFinished"));
                        Console.ResetColor();

                        Console.WriteLine();
                        Console.WriteLine(languageManager.GetText("PressEnterReturn"));
                        Console.ReadLine();
                        break;

                    case "5":
                        Console.Clear();
                        Console.WriteLine(languageManager.GetText("DeleteHeader"));

                        var jobsDelete = configuration.GetJobs();
                        for (int i = 0; i < jobsDelete.Count; i++)
                        {
                            Console.WriteLine($"{jobsDelete[i].Id}. {jobsDelete[i].Name}");
                        }

                        Console.Write(languageManager.GetText("EnterJobNumber"));
                        string inputDelete = Console.ReadLine() ?? string.Empty;

                        if (int.TryParse(inputDelete, out int deleteId))
                        {
                            if (configuration.RemoveJob(deleteId))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(languageManager.GetText("JobDeleted"));
                                Console.ResetColor();
                            }
                            else
                            {
                                view.DisplayError(languageManager.GetText("JobNotFound"));
                            }
                        }
                        else
                        {
                            view.DisplayError(languageManager.GetText("InvalidOption"));
                        }

                        Console.WriteLine();
                        Console.WriteLine(languageManager.GetText("PressEnterReturn"));
                        Console.ReadLine();
                        break;

                    case "6":
                        configuration.LogFormat = view.SelectLogFormat();
                        configuration.SaveConfig();
                        backupService = new BackupService(configuration);
                        break;

                    case "7":
                        keepRunning = false;
                        Console.WriteLine(languageManager.GetText("Goodbye"));
                        break;

                    default:
                        view.DisplayError(languageManager.GetText("InvalidOption"));
                        break;
                }
            }
        }
    }
}