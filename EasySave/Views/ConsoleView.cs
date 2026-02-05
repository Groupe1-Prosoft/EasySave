using System;
using System.Collections.Generic;
using EasySave.Localization;
using EasySave.Models;


namespace EasySave.Views
{
    public class ConsoleView
    {
        private readonly LanguageManager _languageManager;

        public ConsoleView(LanguageManager languageManager)
        {
            _languageManager = languageManager;
        }

        public void ShowHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" ??????????                                  ?????????                                ");
            Console.WriteLine("???????????                                 ???????????                               ");
            Console.WriteLine(" ????  ? ?   ??????    ?????  ????? ????   ????    ???   ??????   ????? ?????  ?????? ");
            Console.WriteLine(" ???????    ????????  ?????  ????? ????    ???????????  ???????? ????? ?????  ????????");
            Console.WriteLine(" ???????     ??????? ???????  ???? ????     ???????????  ???????  ????  ???? ???????? ");
            Console.WriteLine(" ???? ?   ? ????????  ??????? ???? ????     ???    ???? ????????  ????? ???  ???????  ");
            Console.WriteLine(" ???????????????????? ??????  ?????????    ??????????? ??????????  ???????   ???????? ");
            Console.WriteLine("??????????  ???????? ??????    ????????     ?????????   ????????    ?????     ??????  ");
            Console.WriteLine("                               ??? ????                                               ");
            Console.WriteLine("                              ????????                                                ");
            Console.WriteLine("                               ??????                                                 ");
            Console.WriteLine("---------------------------------------");
            Console.ResetColor();
        }

        public void ShowMenu()
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
        }

        public string GetInput()
        {
            return Console.ReadLine() ?? string.Empty;
        }



        public void ShowLanguageSelection()
        {
            Console.Clear();
            Console.WriteLine("EasySave 1.0");
            Console.WriteLine();
            Console.WriteLine("Select language: 1. English  2. Français");
            Console.Write("> ");
        }

        public void ClearAndShowHeader()
        {
            Console.Clear();
            ShowHeader();
        }

        public void DisplayMessage(string message, ConsoleColor color)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        public void DisplayError(string message)
        {
            DisplayMessage(message, ConsoleColor.Red);
        }

        public void DisplaySuccess(string message)
        {
            DisplayMessage(message, ConsoleColor.Green);
        }

        public void WaitUser()
        {
            Console.WriteLine();
            Console.WriteLine(_languageManager.GetText("PressEnterReturn"));
            Console.ReadLine();
        }

        public void DisplayJobList(List<BackupJob> jobs)
        {
            if (jobs == null || jobs.Count == 0)
            {
                Console.WriteLine(_languageManager.GetText("NoJobs"));
            }
            else
            {
                for (int i = 0; i < jobs.Count; i++)
                {
                    var job = jobs[i];
                    Console.WriteLine($"{i + 1}. {job.Name} | {job.Type} | {job.SourceDirectory} -> {job.TargetDirectory}");
                }
            }
        }

        public void DisplayJobSelection(List<BackupJob> jobs)
        {
            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {jobs[i].Name}");
            }
            Console.WriteLine();
        }

        public void SetTitle(string title)
        {
            Console.Title = title;
        }

        public void ShowText(string key)
        {
            Console.WriteLine(_languageManager.GetText(key));
        }

        public void ShowTextWithParam(string key, params object[] args)
        {
            Console.WriteLine(_languageManager.GetText(key, args));
        }

        public void ShowProgress(BackupState state)
        {
            if (state == null) return;

            int progress = 0;
            if (state.TotalFiles > 0)
            {
                progress = (int)(((double)(state.TotalFiles - state.FilesRemaining) / state.TotalFiles) * 100);
            }

            int barWidth = 20;
            int filled = (progress * barWidth) / 100;
            string bar = new string('█', filled) + new string('░', barWidth - filled);

            Console.Write($"\r[{bar}] {progress}% - {state.FilesRemaining} files remaining");
        }

        public string PromptInput(string key)
        {
            Console.Write(_languageManager.GetText(key));
            return Console.ReadLine() ?? string.Empty;
        }
    }
}
