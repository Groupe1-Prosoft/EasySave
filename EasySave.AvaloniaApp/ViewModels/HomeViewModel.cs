using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.AvaloniaApp.Helpers;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.AvaloniaApp.ViewModels;

public partial class SelectableJob : ObservableObject
{
    public BackupJob Job { get; }

    [ObservableProperty]
    private bool _isSelected;

    public SelectableJob(BackupJob job)
    {
        Job = job;
    }
}

public partial class HomeViewModel : ViewModelBase
{
    private readonly Configuration _configuration;
    private readonly BackupService _backupService;

    public ObservableCollection<SelectableJob> Jobs { get; } = new();

    public LocalizationHelper Loc => LocalizationHelper.Instance;

    [ObservableProperty]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newSourceDir = string.Empty;

    [ObservableProperty]
    private string _newTargetDir = string.Empty;

    [ObservableProperty]
    private int _selectedTypeIndex;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isExecuting;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PauseResumeText))]
    private bool _isPaused;

    public string PauseResumeText => IsPaused ? Loc["BtnResume"] : Loc["BtnPause"];

    public HomeViewModel(Configuration configuration, BackupService backupService)
    {
        _configuration = configuration;
        _backupService = backupService;
        RefreshJobs();
    }

    [RelayCommand]
    private void CreateJob()
    {
        var job = new BackupJob
        {
            Name = NewName,
            SourceDir = NewSourceDir,
            TargetDir = NewTargetDir,
            Type = SelectedTypeIndex == 0 ? BackupType.Full : BackupType.Differential
        };

        if (_configuration.AddJob(job))
        {
            NewName = string.Empty;
            NewSourceDir = string.Empty;
            NewTargetDir = string.Empty;
            SelectedTypeIndex = 0;
            StatusMessage = Loc["JobSuccess"];
            RefreshJobs();
        }
        else
        {
            StatusMessage = Loc["JobError"];
        }
    }

    [RelayCommand]
    private async Task ExecuteSelectedAsync()
    {
        var selected = Jobs.Where(j => j.IsSelected).Select(j => j.Job).ToList();
        if (selected.Count == 0) return;

        var ids = selected.Select(j => j.Id).ToList();
        await RunBackupAsync(() => _backupService.ExecuteSequential(ids));
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        var selected = Jobs.Where(j => j.IsSelected).Select(j => j.Job).ToList();
        if (selected.Count == 0) return;

        foreach (var job in selected)
            _configuration.RemoveJob(job.Id);

        RefreshJobs();
    }

    [RelayCommand]
    private async Task ExecuteJobAsync(BackupJob job)
    {
        await RunBackupAsync(() => _backupService.ExecuteJob(job));
    }

    [RelayCommand]
    private void DeleteJob(BackupJob job)
    {
        _configuration.RemoveJob(job.Id);
        RefreshJobs();
    }

    [RelayCommand]
    private void SelectAll()
    {
        bool allSelected = Jobs.All(j => j.IsSelected);
        foreach (var j in Jobs)
            j.IsSelected = !allSelected;
    }

    [RelayCommand]
    private void PauseResume()
    {
        if (!IsExecuting) return;

        if (_backupService.IsPaused)
        {
            _backupService.Resume();
            IsPaused = false;
            StatusMessage = Loc["BackupResumed"];
        }
        else
        {
            _backupService.Pause();
            IsPaused = true;
            StatusMessage = Loc["BackupPaused"];
        }
    }

    [RelayCommand]
    private void Stop()
    {
        if (!IsExecuting) return;

        _backupService.Stop();
        IsPaused = false;
        StatusMessage = Loc["BackupStopped"];
    }

    private async Task RunBackupAsync(Func<bool> execute)
    {
        IsExecuting = true;
        IsPaused = false;
        StatusMessage = Loc["JobExecuting"];

        try
        {
            bool result = await Task.Run(execute);
            if (_backupService.IsStopping)
            {
                StatusMessage = Loc["BackupStopped"];
            }
            else
            {
                StatusMessage = result ? Loc["JobSuccess"] : Loc["JobError"];
            }
        }
        catch (Exception)
        {
            StatusMessage = Loc["JobError"];
        }
        finally
        {
            IsExecuting = false;
            IsPaused = false;
        }
    }

    private void RefreshJobs()
    {
        Jobs.Clear();
        foreach (var job in _configuration.GetJobs())
            Jobs.Add(new SelectableJob(job));
    }
}
