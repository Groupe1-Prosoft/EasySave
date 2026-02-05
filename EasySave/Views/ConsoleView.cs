using System;
using System.Collections.Generic;
using EasySave.Localization;
using EasySave.Models;

namespace EasySave.Views
{
    public class ConsoleView
    {
        private readonly LanguageManager _languageManager;

        // Constructor to initialize the view with the language manager
        public ConsoleView(LanguageManager languageManager)
        {
            _languageManager = languageManager;
        }

        // Displays the application banner in ASCII art
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

        // Clears the screen and displays the main menu options
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

        // Reads a line of text entered by the user
        public string GetInput()
        {
            return Console.ReadLine() ?? string.Empty;
        }

        // Displays the language selection screen at startup
        public void ShowLanguageSelection()
        {
            Console.Clear();
            Console.WriteLine("EasySave 1.0");
            Console.WriteLine();
            Console.WriteLine("Select language: 1. English  2. Français");
            Console.Write("> ");
        }

        // Utility method to clear the console and redisplay the header
        public void ClearAndShowHeader()
        {
            Console.Clear();
            ShowHeader();
        }

        // Helper to display a message with a specific color
        public void DisplayMessage(string message, ConsoleColor color)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        // Displays an error message in red
        public void DisplayError(string message)
        {
            DisplayMessage(message, ConsoleColor.Red);
        }

        // Displays a success message in green
        public void DisplaySuccess(string message)
        {
            DisplayMessage(message, ConsoleColor.Green);
        }

        // Pauses the program and waits for the user to press Enter
        public void WaitUser()
        {
            Console.WriteLine();
            Console.WriteLine(_languageManager.GetText("PressEnterReturn"));
            Console.ReadLine();
        }

        // Displays the list of all configured backup jobs
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

        // Displays a numbered list of jobs for selection
        public void DisplayJobSelection(List<BackupJob> jobs)
        {
            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {jobs[i].Name}");
            }
            Console.WriteLine();
        }

        // Sets the title of the console window
        public void SetTitle(string title)
        {
            Console.Title = title;
        }

        // Displays a localized text based on the key
        public void ShowText(string key)
        {
            Console.WriteLine(_languageManager.GetText(key));
        }

        // Displays a localized text with parameters (like job name or numbers)
        public void ShowTextWithParam(string key, params object[] args)
        {
            Console.WriteLine(_languageManager.GetText(key, args));
        }

        // Displays a visual progress bar based on the backup state
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

        // Displays a prompt message and waits for user input
        public string PromptInput(string key)
        {
            Console.Write(_languageManager.GetText(key));
            return Console.ReadLine() ?? string.Empty;
        }
    }
}