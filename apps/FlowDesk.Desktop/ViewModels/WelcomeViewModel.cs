using CommunityToolkit.Mvvm.Input;
using System;

namespace FlowDesk.Desktop.ViewModels;

public partial class WelcomeViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainViewModel;

    public WelcomeViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    [RelayCommand]
    private void ChoosePrivateMode()
    {
        _mainViewModel.NavigateTo(new CreatePrivateWorkspaceViewModel(_mainViewModel));
    }
}
