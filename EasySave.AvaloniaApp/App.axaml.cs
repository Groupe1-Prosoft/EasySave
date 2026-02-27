using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using EasySave.AvaloniaApp.ViewModels;
using EasySave.AvaloniaApp.Views;
using EasySave.Models;
using EasySave.Services;
using EasyLog;

namespace EasySave.AvaloniaApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            

            // 1. Create the configuration
            var configuration = new Configuration();
            configuration.LoadConfig();

            // 2. Create services and provide the configuration
            // FIXED LINE HERE: Enable network ("both") and add Docker URL
            ILogger logger = new Logger(configuration.LogFormat, "both", "http://localhost:8080");

            var businessMonitor = new BusinessSoftwareMonitor();
            var cryptoService = new CryptoSoftService();

            // Inject all dependencies into the BackupService
            var backupService = new BackupService(configuration, logger, cryptoService, businessMonitor);

            // 3. Create sub-view models (Home and Settings)
            var homeViewModel = new HomeViewModel(configuration, backupService);
            var settingsViewModel = new SettingsViewModel(configuration);

            // 4. Inject sub-view models into the Main Menu (MainWindowViewModel)
            var mainWindowViewModel = new MainWindowViewModel(homeViewModel, settingsViewModel);

            

            // Launch the graphical window with our ready-to-use ViewModel!
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}