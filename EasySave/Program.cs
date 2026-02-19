using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Application entry point.
        /// </summary>
        public static void Main(string[] args)
        {
            // Create service container (handles ALL dependency injection)
            var services = new ServiceContainer();

            // Command-line mode: execute jobs from arguments
            if (args.Length > 0)
            {
                var cmd = services.GetCommandLineService();
                List<int> ids = cmd.ParseArgument(args[0]);
                services.GetBackupService().ExecuteSequential(ids);
                return;
            }

            // Interactive menu mode (delegates to MenuController)
            var menuController = new MenuController(services);
            menuController.Run();
        }
    }
}