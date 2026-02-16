using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.AvaloniaApp.Helpers;

namespace EasySave.AvaloniaApp.ViewModels;

public partial class LogsViewModel : ViewModelBase
{
    public ObservableCollection<LogEntry> Logs { get; } = new();

    public LocalizationHelper Loc => LocalizationHelper.Instance;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    private void LoadLogs()
    {
        Logs.Clear();
        string logsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "Logs");

        if (!Directory.Exists(logsDir))
        {
            StatusMessage = Loc["LogsNoLogs"];
            return;
        }

        var files = Directory.GetFiles(logsDir, "*.json");
        foreach (var file in files)
        {
            try
            {
                string content = File.ReadAllText(file);
                // Log files may contain a JSON array of entries
                var entries = JsonSerializer.Deserialize<LogEntry[]>(content);
                if (entries != null)
                {
                    foreach (var entry in entries)
                        Logs.Add(entry);
                }
            }
            catch
            {
                // Skip malformed files
            }
        }

        // Also try XML log files
        var xmlFiles = Directory.GetFiles(logsDir, "*.xml");
        foreach (var file in xmlFiles)
        {
            try
            {
                string content = File.ReadAllText(file);
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(LogEntry[]));
                using var reader = new StringReader(content);
                var entries = serializer.Deserialize(reader) as LogEntry[];
                if (entries != null)
                {
                    foreach (var entry in entries)
                        Logs.Add(entry);
                }
            }
            catch
            {
                // Skip malformed files
            }
        }

        StatusMessage = Logs.Count == 0 ? Loc["LogsNoLogs"] : string.Empty;
    }
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public long Size { get; set; }
    public long TransferTime { get; set; }
    public long EncryptionTime { get; set; }
}
