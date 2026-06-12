using CommunityToolkit.Mvvm.ComponentModel;

namespace FlowDesk.Desktop.ViewModels;

public partial class PlaceholderViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _moduleName;

    public PlaceholderViewModel(string moduleName)
    {
        _moduleName = moduleName;
    }
}
