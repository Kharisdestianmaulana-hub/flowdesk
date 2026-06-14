using CommunityToolkit.Mvvm.Input;

namespace FlowDesk.Desktop.ViewModels;

public partial class LocalWorkspaceSelectionViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainViewModel;

    public LocalWorkspaceSelectionViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    [RelayCommand]
    private void HostNewWorkspace()
    {
        _mainViewModel.NavigateTo(new CreatePrivateWorkspaceViewModel(_mainViewModel, true));
    }

    [RelayCommand]
    private void JoinExistingWorkspace()
    {
        _mainViewModel.NavigateTo(new JoinWorkspaceViewModel(_mainViewModel));
    }

    [RelayCommand]
    private void GoBack()
    {
        _mainViewModel.NavigateTo(new WelcomeViewModel(_mainViewModel));
    }
}
