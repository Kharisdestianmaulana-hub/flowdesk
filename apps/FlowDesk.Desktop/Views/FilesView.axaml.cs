using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using FlowDesk.Desktop.ViewModels;
using System.Linq;

namespace FlowDesk.Desktop.Views;

public partial class FilesView : UserControl
{
    public FilesView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragOverEvent, DragOver);
        AddHandler(DragDrop.DropEvent, Drop);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
    }

    private void DragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer != null && e.DataTransfer.TryGetFiles() != null)
        {
            e.DragEffects = DragDropEffects.Copy;
            this.FindControl<Border>("DropZoneOverlay")!.IsVisible = true;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        this.FindControl<Border>("DropZoneOverlay")!.IsVisible = false;
    }

    private async void Drop(object? sender, DragEventArgs e)
    {
        this.FindControl<Border>("DropZoneOverlay")!.IsVisible = false;

        if (DataContext is FilesViewModel vm)
        {
            if (e.DataTransfer != null)
            {
                var files = e.DataTransfer.TryGetFiles();
                if (files != null && files.Any())
                {
                    var paths = files.Select(f => f.TryGetLocalPath()).Where(p => !string.IsNullOrEmpty(p)).Cast<string>().ToList();
                    if (paths.Any())
                    {
                        await vm.ImportFilesAsync(paths);
                    }
                }
            }
        }
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is FilesViewModel vm)
        {
            vm.OpenFilePickerAsync = async () =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "Import File",
                        AllowMultiple = false
                    });
                    return files.Count >= 1 ? files[0] : null;
                }
                return null;
            };
        }
    }
}
