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

    public MainWindowViewModel()
    {
        // Instanciation de la configuration / service — adapte si votre projet expose un singleton/factory
        _configuration = new Configuration();
        // Charger la configuration depuis le disque avant d'accéder aux jobs
        _configuration.LoadConfig();

        _backupService = new BackupService(_configuration);

        var jobs = _configuration.GetJobs() ?? Array.Empty<BackupJob>();
        Jobs = new ObservableCollection<BackupJob>(jobs);
    }

    [ObservableProperty]
    private ObservableCollection<BackupJob> jobs = new();

    [ObservableProperty]
    private BackupJob? selectedJob;

    [RelayCommand]
    private void CreateJob(BackupJob? job)
    {
        if (job is null) return;

        // AddJob retourne bool ; Configuration gère l'assignation d'Id et la persistance
        var ok = _configuration.AddJob(job);
        if (ok)
        {
            Jobs.Add(job);
        }
        else
        {
            // Optionnel: gérer l'erreur (affichage, log...)
        }
    }

    [RelayCommand]
    private void DeleteJob()
    {
        if (SelectedJob is null) return;

        // Configuration expose RemoveJob(int id)
        _configuration.RemoveJob(SelectedJob.Id);
        Jobs.Remove(SelectedJob);
        SelectedJob = null;
    }

    [RelayCommand]
    private async Task ExecuteJobAsync()
    {
        if (SelectedJob is null) return;

        // Exécution dans un thread de fond pour ne pas bloquer l'UI
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
        // Le LanguageManager ne notifie pas automatiquement : le bouton 🔄 qui recrée le VM reste nécessaire
    }

    [RelayCommand]
    private void SaveSettings()
    {
        // Utilise la méthode existante de Configuration
        _configuration.SaveConfig();
    }
}
