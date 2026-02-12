using System;
using System.Collections.Generic;
using EasySave.Localization;
using EasySave.Models;

namespace EasySave.Views
{
    /// <summary>
    /// Handles all console UI rendering and input helpers.
    /// </summary>
    public class ConsoleView
    {
        private readonly LanguageManager _languageManager;

        /// <summary>
        /// Initializes the console view with a language manager.
        /// </summary>
        public ConsoleView(LanguageManager languageManager)
        {
            _languageManager = languageManager;
        }

        /// <summary>
        /// Displays the ASCII art application banner.
        /// </summary>
        public void ShowHeader()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"                                                                                               ");
            Console.WriteLine(@"    ,---,.                                             .--.--.                                 ");
            Console.WriteLine(@"  ,'  .' |                                            /  /    '.                                ");
            Console.WriteLine(@",---.'   |                                           |  :  /`. /                                ");
            Console.WriteLine(@"|   |   .'               .--.--.                     ;  |  |--`                 .---.          ");
            Console.WriteLine(@":   :  |-,   ,--.--.    /  /    '       .--,         |  :  ;_      ,--.--.    /.  ./|  ,---.   ");
            Console.WriteLine(@":   |  ;/|  /       \  |  :  /`./     /_ ./|          \  \    `.  /       \ .-' . ' | /     \  ");
            Console.WriteLine(@"|   :   .' .--.  .-. | |  :  ;_    , ' , ' :           `----.   \.--.  .-. /___/ \: |/    /  | ");
            Console.WriteLine(@"|   |  |-,  \__\/: . .  \  \    `./___/ \: |           __ \  \  | \__\/: . .   \  ' .    ' / | ");
            Console.WriteLine(@"'   :  ;/|  ,"" .--.; |   `----.   \.  \  ' |          /  /`--'  / ,"" .--.; |\   \   '   ;   /| ");
            Console.WriteLine(@"|   |    \ /  /  ,.  |  /  /`--'  / \  ;   :         '--'.     / /  /  ,.  | \   \  '   |  / | ");
            Console.WriteLine(@"|   :   .';  :   .'   \'--'.     /   \  \  ;           `--'---' ;  :   .'   \ \   \ |   :    | ");
            Console.WriteLine(@"|   | ,'  |  ,     .-./  `--'---'     :  \  \                   |  ,     .-./  '---"" \   \  /  ");
            Console.WriteLine(@"`---.'     `--`---'                    \  ' ;                    `--`---'             `----'   ");
            Console.WriteLine(@"                                        `--`                                                    ");
            Console.WriteLine("---------------------------------------");
            Console.ResetColor();
        }

        /// <summary>
        /// Displays the main menu and prompts for the user choice.
        /// </summary>
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

        /// <summary>
        /// Reads a single line from the console.
        /// </summary>
        public string GetInput()
        {
            return Console.ReadLine() ?? string.Empty;
        }

        /// <summary>
        /// Displays the language selection prompt at startup.
        /// </summary>
        public void ShowLanguageSelection()
        {
            Console.Clear();
            Console.WriteLine("EasySave 1.0");
            Console.WriteLine();
            Console.WriteLine("Select language: 1. English  2. Français");
            Console.Write("> ");
        }

        /// <summary>
        /// Clears the console and reprints the header.
        /// </summary>
        public void ClearAndShowHeader()
        {
            Console.Clear();
            ShowHeader();
        }

        /// <summary>
        /// Displays a colored message without pausing.
        /// </summary>
        public void DisplayMessage(string message, ConsoleColor color)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Displays an error message in red.
        /// </summary>
        public void DisplayError(string message)
        {
            DisplayMessage(message, ConsoleColor.Red);
        }

        /// <summary>
        /// Displays a success message in green.
        /// </summary>
        public void DisplaySuccess(string message)
        {
            DisplayMessage(message, ConsoleColor.Green);
        }

        /// <summary>
        /// Pauses execution until Enter is pressed.
        /// </summary>
        public void WaitUser()
        {
            Console.WriteLine();
            Console.WriteLine(_languageManager.GetText("PressEnterReturn"));
            Console.ReadLine();
        }

        /// <summary>
        /// Displays the list of configured backup jobs.
        /// </summary>
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

        /// <summary>
        /// Displays a numbered list for job selection.
        /// </summary>
        public void DisplayJobSelection(List<BackupJob> jobs)
        {
            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {jobs[i].Name}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Sets the console window title.
        /// </summary>
        public void SetTitle(string title)
        {
            Console.Title = title;
        }

        /// <summary>
        /// Displays a localized string by key.
        /// </summary>
        public void ShowText(string key)
        {
            Console.WriteLine(_languageManager.GetText(key));
        }

        /// <summary>
        /// Displays a localized string with formatting parameters.
        /// </summary>
        public void ShowTextWithParam(string key, params object[] args)
        {
            Console.WriteLine(_languageManager.GetText(key, args));
        }

        /// <summary>
        /// Renders a progress bar for the provided state.
        /// </summary>
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

        /// <summary>
        /// Prompts for a localized input and returns the user entry.
        /// </summary>
        public string PromptInput(string key)
        {
            Console.Write(_languageManager.GetText(key));
            return Console.ReadLine() ?? string.Empty;
        }
    }
}