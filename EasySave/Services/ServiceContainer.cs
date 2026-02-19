using System;
using EasySave.Models;
using EasySave.Views;
using EasySave.Localization;
using EasyLog;

namespace EasySave.Services
{
    /// <summary>
    /// Service container for dependency injection and object creation.
    /// Centralizes all instantiation logic in one place (Factory pattern).
    /// Goal: Enable true Dependency Injection instead of "new Logger()" inside constructors.
    /// </summary>
    public class ServiceContainer
    {
        private readonly LanguageManager _languageManager;
        private readonly Configuration _configuration;
        private readonly ConsoleView _view;
        private ILogger _logger;
        private BackupService _backupService;
        private readonly CommandLineService _commandLineService;
        private readonly BusinessSoftwareMonitor _businessSoftwareMonitor;
        private readonly CryptoSoftService _cryptoSoftService;

        /// <summary>
        /// Initializes all services with proper dependency injection chain.
        /// This is the ONLY place where objects are created.
        /// </summary>
        public ServiceContainer()
        {
            // 1. Load configuration first (no dependencies)
            _configuration = new Configuration();
            _configuration.LoadConfig();

            // 2. Initialize language manager (singleton, no dependencies)
            _languageManager = LanguageManager.Instance;

            // 3. Create logger (depends on configuration.LogFormat)
            _logger = new Logger(_configuration.LogFormat);

            // 4. Create business monitor and crypto service (no dependencies)
            _businessSoftwareMonitor = new BusinessSoftwareMonitor();
            _cryptoSoftService = new CryptoSoftService();

            // 5. Create view (depends on language manager)
            _view = new ConsoleView(_languageManager);

            // 6. Create backup service (inject ALL dependencies, don't create them inside)
            _backupService = new BackupService(
                _configuration,
                _logger,
                _cryptoSoftService,
                _businessSoftwareMonitor);

            // 7. Command line service (no dependencies)
            _commandLineService = new CommandLineService();
        }

        /// <summary>
        /// Gets the configuration instance.
        /// </summary>
        public Configuration GetConfiguration() => _configuration;

        /// <summary>
        /// Gets the language manager instance.
        /// </summary>
        public LanguageManager GetLanguageManager() => _languageManager;

        /// <summary>
        /// Gets the console view instance.
        /// </summary>
        public ConsoleView GetConsoleView() => _view;

        /// <summary>
        /// Gets the backup service instance.
        /// </summary>
        public BackupService GetBackupService() => _backupService;

        /// <summary>
        /// Gets the command line service instance.
        /// </summary>
        public CommandLineService GetCommandLineService() => _commandLineService;

        /// <summary>
        /// Refreshes the backup service after configuration changes (e.g., log format changed).
        /// Re-creates logger and backup service with updated configuration.
        /// </summary>
        public void RefreshBackupService()
        {
            _logger = new Logger(_configuration.LogFormat);
            _backupService = new BackupService(
                _configuration,
                _logger,
                _cryptoSoftService,
                _businessSoftwareMonitor);
        }

        /// <summary>
        /// Gets the logger instance (useful for advanced scenarios).
        /// </summary>
        public ILogger GetLogger() => _logger;
    }
}