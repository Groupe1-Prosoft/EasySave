using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using EasySave.AvaloniaApp.ViewModels;
using EasySave.Models;
using System;

namespace EasySave.AvaloniaApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // Permet de déplacer la fenêtre en cliquant sur la "fausse" barre de titre
    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // Lance le déplacement natif de la fenêtre
            BeginMoveDrag(e);
        }
    }

    // Ouvre ou ferme le menu latéral (SplitView)
    private void BtnToggleSidebar_OnClick(object? sender, RoutedEventArgs e)
    {
        MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
    }

    // HACK POUR LE CHANGEMENT DE LANGUE
    // Force le rechargement du ViewModel pour appliquer la nouvelle langue
    private void BtnReloadUI_OnClick(object? sender, RoutedEventArgs e)
    {
        // On détache le ViewModel actuel
        this.DataContext = null;
        // On en recrée un nouveau, ce qui forcera l'interface à relire tous les textes
        // via le LanguageManager (mis à jour par les boutons FR/EN).
        this.DataContext = new MainWindowViewModel();

        // Petit hack visuel pour forcer le rafraîchissement du layout si nécessaire
        this.InvalidateVisual();
    }

    // DIALOGS DE FICHIERS (BROWSE) ------------------------------------------------

    private async void BtnBrowseSource_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Sélectionner le dossier Source",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            NewJobSourceTb.Text = folders[0].Path.LocalPath;
        }
    }

    private async void BtnBrowseTarget_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Sélectionner le dossier de Destination",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            NewJobTargetTb.Text = folders[0].Path.LocalPath;
        }
    }

    // CRÉATION DE TÂCHE ----------------------------------------------------------

    private void BtnCreateJob_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewJobNameTb.Text) ||
            string.IsNullOrWhiteSpace(NewJobSourceTb.Text) ||
            string.IsNullOrWhiteSpace(NewJobTargetTb.Text))
        {
            return;
        }

        if (DataContext is not MainWindowViewModel vm) return;

        var selectedType = NewJobTypeCb.SelectedIndex == 0 ? BackupType.Full : BackupType.Differential;

        var newJob = new BackupJob
        {
            Name = NewJobNameTb.Text.Trim(),
            SourceDir = NewJobSourceTb.Text.Trim(),
            TargetDir = NewJobTargetTb.Text.Trim(),
            Type = selectedType
        };

        if (vm.CreateJobCommand.CanExecute(newJob))
        {
            vm.CreateJobCommand.Execute(newJob);

            NewJobNameTb.Text = string.Empty;
            NewJobSourceTb.Text = string.Empty;
            NewJobTargetTb.Text = string.Empty;
            NewJobTypeCb.SelectedIndex = 0;
        }
    }
}