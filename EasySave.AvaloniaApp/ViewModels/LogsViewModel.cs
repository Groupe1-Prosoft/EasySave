using System;
using System.Collections.Generic;
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

    public LogsViewModel()
    {
        LoadLogs();
    }

    [RelayCommand]
    private void LoadLogs()
    {
        Logs.Clear();
        string logsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "Logs");

        if (!Directory.Exists(logsDir))
        {
            StatusMessage = $"Log directory not found: {logsDir}";
            return;
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var files = Directory.GetFiles(logsDir, "*.json");
        foreach (var file in files)
        {
            try
            {
                string content = File.ReadAllText(file);
                var entries = JsonSerializer.Deserialize<List<LogEntry>>(content, jsonOptions);
                if (entries != null)
                {
                    foreach (var entry in entries)
                        Logs.Add(entry);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error reading {Path.GetFileName(file)}: {ex.Message}";
            }
        }

        // Also try XML log files
        var xmlFiles = Directory.GetFiles(logsDir, "*.xml");
        foreach (var file in xmlFiles)
        {
            try
            {
                string content = File.ReadAllText(file);
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<LogEntry>));
                using var reader = new StringReader(content);
                var entries = serializer.Deserialize(reader) as List<LogEntry>;
                if (entries != null)
                {
                    foreach (var entry in entries)
                        Logs.Add(entry);
                }
            }
            catch
            {
                // XML format may differ, skip silently
            }
        }

        if (Logs.Count == 0 && string.IsNullOrEmpty(StatusMessage))
            StatusMessage = Loc["LogsNoLogs"];
        else if (Logs.Count > 0)
            StatusMessage = string.Empty;
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
