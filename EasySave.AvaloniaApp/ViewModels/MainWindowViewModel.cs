using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.AvaloniaApp.Helpers;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.AvaloniaApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly Configuration _configuration;
    private readonly BackupService _backupService;

    // Child view models
    private readonly HomeViewModel _homeViewModel;
    private readonly SettingsViewModel _settingsViewModel;

    public LocalizationHelper Loc => LocalizationHelper.Instance;

    [ObservableProperty]
    private bool _isPaneOpen;

    [ObservableProperty]
    private ViewModelBase _currentPage;

    public MainWindowViewModel()
    {
        _configuration = new Configuration();
        _configuration.LoadConfig();
        _backupService = new BackupService(_configuration);

        _homeViewModel = new HomeViewModel(_configuration, _backupService);
        _settingsViewModel = new SettingsViewModel(_configuration);

        _currentPage = _homeViewModel;
    }

    [RelayCommand]
    private void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    [RelayCommand]
    private void Navigate(string page)
    {
        CurrentPage = page switch
        {
            "Home" => _homeViewModel,
            "Settings" => _settingsViewModel,
            _ => _homeViewModel
        };


    }


    [RelayCommand]
    private void SwitchLanguage(string lang)
    {
        Loc.SwitchLanguage(lang);
    }
}
