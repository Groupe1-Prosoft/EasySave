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

            // --- DEBUT DU COMPOSITION ROOT (L'Injection de Dépendances) ---

            // 1. On fabrique la configuration
            var configuration = new Configuration();
            configuration.LoadConfig();

            // 2. On fabrique les services en leur donnant la configuration
            ILogger logger = new Logger(configuration.LogFormat);
            var businessMonitor = new BusinessSoftwareMonitor();
            var cryptoService = new CryptoSoftService();

            // On donne tout au BackupService
            var backupService = new BackupService(configuration, logger, cryptoService, businessMonitor);

            // 3. On fabrique les sous-menus (Home et Settings)
            var homeViewModel = new HomeViewModel(configuration, backupService);
            var settingsViewModel = new SettingsViewModel(configuration);

            // 4. On donne les sous-menus au Menu Principal (MainWindowViewModel)
            var mainWindowViewModel = new MainWindowViewModel(homeViewModel, settingsViewModel);

            // --- FIN DU COMPOSITION ROOT ---

            // On lance la fenêtre graphique avec notre ViewModel tout prêt !
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