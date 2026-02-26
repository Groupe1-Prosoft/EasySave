using System;
using System.Collections.Generic;
using System.Threading;
using EasySave.Services;

namespace EasySave
{
    /// <summary>
    /// Console entry point for EasySave backup application.
    /// Orchestrates dependency injection and menu flow.
    /// NOW: Just 15 lines (delegating to ServiceContainer and MenuController).
    /// BEFORE: 250 lines with all menu logic mixed in.
    /// </summary>
    class Program
    {
        // Unique identifier for the application Mutex
        private const string MutexName = "Global\\EasySaveAppMutex";

        /// <summary>
        /// Application entry point.
        /// </summary>
        public static void Main(string[] args)
        {
            // Create a Mutex to ensure only one instance of EasySave is running
            using (Mutex mutex = new Mutex(true, MutexName, out bool createdNew))
            {
                // If the Mutex already exists, another instance is running
                if (!createdNew)
                {
                    Console.WriteLine("Another instance of EasySave is already running.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return; // Exit the application immediately
                }

                // Create service container (handles ALL dependency injection)
                var services = new ServiceContainer();

                // Command-line mode: execute jobs from arguments
                if (args.Length > 0)
                {
                    var cmd = services.GetCommandLineService();
                    List<int> ids = cmd.ParseArgument(args[0]);
                    services.GetBackupService().ExecuteParallel(ids);
                    return;
                }

                // Interactive menu mode (delegates to MenuController)
                var menuController = new MenuController(services);
                menuController.Run();
            }
        }
    }
}