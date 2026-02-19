using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.AvaloniaApp.Helpers;
using EasySave.Models;
using EasySave.Services;
using EasyLog;

namespace EasySave.AvaloniaApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly Configuration _configuration;
    private readonly BackupService _backupService;

    // Child view models
    private readonly HomeViewModel _homeViewModel;
    private readonly SettingsViewModel _settingsViewModel;

    /// <summary>
    /// Gets the localization helper instance.
    /// </summary>
    public LocalizationHelper Loc => LocalizationHelper.Instance;

    /// <summary>
    /// Gets or sets whether the navigation pane is open.
    /// </summary>
    [ObservableProperty]
    private bool _isPaneOpen;

    /// <summary>
    /// Gets or sets the current page view model.
    /// </summary>
    [ObservableProperty]
    private ViewModelBase _currentPage;

    /// <summary>
    /// Initializes the main window view model with all services.
    /// </summary>
    public MainWindowViewModel()
    {
        // Load configuration
        _configuration = new Configuration();
        _configuration.LoadConfig();

        // Inject all dependencies into BackupService (true DI)
        ILogger logger = new Logger(_configuration.LogFormat);
        var businessMonitor = new BusinessSoftwareMonitor();
        var cryptoService = new CryptoSoftService();

        _backupService = new BackupService(_configuration, logger, cryptoService, businessMonitor);

        _homeViewModel = new HomeViewModel(_configuration, _backupService);
        _settingsViewModel = new SettingsViewModel(_configuration);

        CurrentPage = _homeViewModel;
    }

    /// <summary>
    /// Toggles the navigation pane open/closed.
    /// </summary>
    [RelayCommand]
    private void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    /// <summary>
    /// Navigates to a specific page by name.
    /// </summary>
    [RelayCommand]
    private void Navigate(string page)
    {
        CurrentPage = page switch
        {
            "Home" => _homeViewModel,
            "Settings" => _settingsViewModel,
            _ => CurrentPage
        };
    }

    /// <summary>
    /// Switches the application language.
    /// </summary>
    [RelayCommand]
    private void SwitchLanguage(string language)
    {
        LocalizationHelper.Instance.SwitchLanguage(language);
    }
}