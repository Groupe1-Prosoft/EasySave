using System.ComponentModel;
using EasySave.Localization;

namespace EasySave.AvaloniaApp.Helpers;

public sealed class LocalizationHelper : INotifyPropertyChanged
{
    private static readonly LocalizationHelper _instance = new();
    public static LocalizationHelper Instance => _instance;

    public event PropertyChangedEventHandler? PropertyChanged;

    private LocalizationHelper() { }

    public string this[string key] => LanguageManager.Instance.GetText(key);

    public string CurrentLanguage => LanguageManager.Instance.CurrentLanguage;

    public void SwitchLanguage(string lang)
    {
        LanguageManager.Instance.SetLanguage(lang);
        // Fire change for all bindings using the indexer
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
    }
}
