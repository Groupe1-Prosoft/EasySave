using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Localization;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.AvaloniaApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly Configuration _configuration;
    private readonly BackupService _backupService;

    public MainWindowViewModel()
    {
        // Instanciation de la configuration / service — adapte si votre projet expose un singleton/factory
        _configuration = new Configuration();
        _backupService = new BackupService(_configuration);

        var jobs = _configuration.GetJobs() ?? Array.Empty<BackupJob>();
        Jobs = new ObservableCollection<BackupJob>(jobs);
    }

    [ObservableProperty]
    private ObservableCollection<BackupJob> jobs = new();

    [ObservableProperty]
    private BackupJob? selectedJob;

    // CREATE: param is BackupJob built by View (code-behind) as requested in the briefing
    [RelayCommand]
    private void CreateJob(BackupJob? job)
    {
        if (job is null) return;

        // Configuration is responsible for assigning Id, etc.
        // Méthode attendue : AddJob(BackupJob) — adaptez si le nom diffère (ex: CreateJob, SaveJob)
        _configuration.AddJob(job);

        Jobs.Add(job);
        SaveSettings();
    }

    [RelayCommand]
    private void DeleteJob()
    {
        if (SelectedJob is null) return;

        // Confirm deletion can be implemented in the View if needed.
        // Méthode attendue : RemoveJob(BackupJob) ou RemoveJobById(int)
        _configuration.RemoveJob(SelectedJob);
        Jobs.Remove(SelectedJob);
        SelectedJob = null;

        SaveSettings();
    }

    [RelayCommand]
    private async Task ExecuteJobAsync()
    {
        if (SelectedJob is null) return;

        // Exécution bloquante déplacée dans un Task.Run pour ne pas bloquer l'UI (conseil du briefing)
        await Task.Run(() => _backupService.ExecuteJob(SelectedJob));
    }

    [RelayCommand]
    private async Task ExecuteSequentialAsync()
    {
        var ids = Jobs.Select(j => j.Id).ToList();
        await Task.Run(() => _backupService.ExecuteSequential(ids));
    }

    [RelayCommand]
    private void ChangeLanguage(string? lang)
    {
        if (string.IsNullOrWhiteSpace(lang)) return;
        LanguageManager.Instance.SetLanguage(lang);
        // Le LanguageManager ne notifie pas : l'UI doit être rechargée (tu as déjà le bouton 🔄 qui recrée le VM)
    }

    [RelayCommand]
    private void SaveSettings()
    {
        // Méthode attendue : Save() ou Persist() — adapte si le nom diffère
        _configuration.Save();
    }
}
