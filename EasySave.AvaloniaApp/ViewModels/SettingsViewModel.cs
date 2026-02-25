using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.AvaloniaApp.Helpers;
using EasySave.Models;

namespace EasySave.AvaloniaApp.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly Configuration _configuration;

    public LocalizationHelper Loc => LocalizationHelper.Instance;

    [ObservableProperty]
    private string _cryptoSoftPath = string.Empty;

    [ObservableProperty]
    private string _extensionsText = string.Empty;

    [ObservableProperty]
    private string _businessSoftwareName = string.Empty;

    [ObservableProperty]
    private int _selectedLogFormatIndex;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private long _maxLargeFileSizeKB;

    [ObservableProperty]
    private string _priorityExtensionsText = string.Empty;



    public SettingsViewModel(Configuration configuration)
    {
        _configuration = configuration;
        LoadFromConfig();
    }

    private void LoadFromConfig()
    {
        // Utilise l'API existante de Configuration
        CryptoSoftPath = _configuration.CryptoSoftPath;
        ExtensionsText = string.Join(", ", _configuration.ExtensionsToEncrypt ?? new List<string>());
        BusinessSoftwareName = _configuration.GetBusinessSoftwareName();
        SelectedLogFormatIndex = (_configuration.LogFormat ?? "json").ToLower() == "xml" ? 1 : 0;
        MaxLargeFileSizeKB = _configuration.MaxLargeFileSizeKB;
        PriorityExtensionsText = string.Join(", ", _configuration.PriorityExtensions ?? new List<string>());


    }

    [RelayCommand]
    private void Save()
    {
        _configuration.CryptoSoftPath = CryptoSoftPath;

        var extensions = ExtensionsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        _configuration.ExtensionsToEncrypt = extensions;
        _configuration.SetBusinessSoftwareName(BusinessSoftwareName);
        _configuration.LogFormat = SelectedLogFormatIndex == 1 ? "xml" : "json";
        _configuration.MaxLargeFileSizeKB = MaxLargeFileSizeKB;
        var priorityExtensions = PriorityExtensionsText
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .ToList();
        _configuration.PriorityExtensions = priorityExtensions;




        _configuration.SaveConfig();
        StatusMessage = Loc["SettingsSaved"];
    }
}
