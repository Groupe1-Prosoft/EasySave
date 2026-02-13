using System;
using System.Collections.Generic;
using EasySave.Localization;
using EasySave.Models;

namespace EasySave.Views
{
    /// <summary>
    /// Handles the minimal console UI required by the class diagram.
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
        /// Displays the main menu and prompts for the user choice.
        /// </summary>
        public void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine(_languageManager.GetText("SelectOption"));
            Console.WriteLine(_languageManager.GetText("ListJobs"));
            Console.WriteLine(_languageManager.GetText("CreateJob"));
            Console.WriteLine(_languageManager.GetText("ExecuteJob"));
            Console.WriteLine(_languageManager.GetText("ExecuteAllJobs"));
            Console.WriteLine(_languageManager.GetText("DeleteJob"));
            Console.WriteLine(_languageManager.GetText("Options"));
            Console.WriteLine(_languageManager.GetText("Exit"));
            Console.WriteLine(_languageManager.GetText("Separator"));
            Console.Write(_languageManager.GetText("YourChoice"));
        }

        /// <summary>
        /// Prompts the user to select the log format.
        /// </summary>
        public string SelectLogFormat()
        {
            Console.WriteLine("1. Format JSON (Défaut)");
            Console.WriteLine("2. Format XML");
            Console.Write("> ");
            string choice = Console.ReadLine() ?? string.Empty;
            return choice.Trim().Equals("2") || choice.Trim().Equals("xml", StringComparison.OrdinalIgnoreCase)
                ? "xml"
                : "json";
        }

        /// <summary>
        /// Reads a single line from the console.
        /// </summary>
        public string GetInput()
        {
            return Console.ReadLine() ?? string.Empty;
        }

        /// <summary>
        /// Displays a progress bar based on the provided state.
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
        /// Displays an error message in red.
        /// </summary>
        public void DisplayError(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }
    }
}