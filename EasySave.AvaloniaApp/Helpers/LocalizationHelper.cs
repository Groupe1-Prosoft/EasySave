using System.ComponentModel;
using EasySave.Localization;

namespace EasySave.AvaloniaApp.Helpers;

/// <summary>
/// Lightweight proxy returned by ViewModels as their Loc property.
/// A NEW instance is created on each language switch, forcing Avalonia
/// to re-evaluate all {Binding Loc[Key]} bindings (reference change).
/// </summary>
public class LanguageProxy
{
    public string this[string key] => LanguageManager.Instance.GetText(key);
}

public sealed class LocalizationHelper : INotifyPropertyChanged
{
    private static readonly LocalizationHelper _instance = new();
    public static LocalizationHelper Instance => _instance;

    public event PropertyChangedEventHandler? PropertyChanged;

    private LocalizationHelper() { }

    public string CurrentLanguage => LanguageManager.Instance.CurrentLanguage;

    public void SwitchLanguage(string lang)
    {
        LanguageManager.Instance.SetLanguage(lang);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }
}
