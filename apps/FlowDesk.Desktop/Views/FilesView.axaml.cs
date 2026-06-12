using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FlowDesk.Desktop.ViewModels;

namespace FlowDesk.Desktop.Views;

public partial class FilesView : UserControl
{
    public FilesView()
    {
        InitializeComponent();
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
