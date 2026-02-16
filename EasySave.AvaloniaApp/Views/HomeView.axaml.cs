using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using EasySave.AvaloniaApp.ViewModels;

namespace EasySave.AvaloniaApp.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }

    private async void BtnBrowseSource_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Select source folder"
        });

        if (folders.Count > 0 && DataContext is HomeViewModel vm)
        {
            vm.NewSourceDir = folders[0].Path.LocalPath;
        }
    }

    private async void BtnBrowseTarget_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Select target folder"
        });

        if (folders.Count > 0 && DataContext is HomeViewModel vm)
        {
            vm.NewTargetDir = folders[0].Path.LocalPath;
        }
    }
}
