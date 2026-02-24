using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.AvaloniaApp.Helpers;
using EasySave.Models;
using EasySave.Services;
using EasyLog;

namespace EasySave.AvaloniaApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly HomeViewModel _homeViewModel;
    private readonly SettingsViewModel _settingsViewModel;

    public LocalizationHelper Loc => LocalizationHelper.Instance;

    [ObservableProperty]
    private bool _isPaneOpen;

    [ObservableProperty]
    private ViewModelBase _currentPage;

    // Le constructeur demande juste les deux pages, il ne fabrique plus rien lui-même
    public MainWindowViewModel(HomeViewModel homeViewModel, SettingsViewModel settingsViewModel)
    {
        _homeViewModel = homeViewModel;
        _settingsViewModel = settingsViewModel;
        CurrentPage = _homeViewModel;
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
            _ => CurrentPage
        };
    }

    [RelayCommand]
    private void SwitchLanguage(string language)
    {
        LocalizationHelper.Instance.SwitchLanguage(language);
    }
}