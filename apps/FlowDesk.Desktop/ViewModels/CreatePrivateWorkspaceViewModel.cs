using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace FlowDesk.Desktop.ViewModels;

public partial class CreatePrivateWorkspaceViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainViewModel;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _workspaceName = string.Empty;

    [ObservableProperty]
    private string _workspaceType = "General Team";

    [ObservableProperty]
    private string _pageTitle = "Create Private Workspace";

    [ObservableProperty]
    private string _buttonTitle = "Create Workspace";

    private bool _isLocalMode;

    public CreatePrivateWorkspaceViewModel(MainWindowViewModel mainViewModel, bool isLocalMode = false)
    {
        _mainViewModel = mainViewModel;
        _isLocalMode = isLocalMode;

        if (_isLocalMode)
        {
            PageTitle = "Create Local Workspace";
            ButtonTitle = "Create Local Workspace";
        }
    }

    [RelayCommand]
    private void CreateWorkspace()
    {
        if (string.IsNullOrWhiteSpace(WorkspaceName)) return;

        using var db = new FlowDesk.Infrastructure.Data.FlowDeskDbContext();
        
        var workspace = new FlowDesk.Core.Models.Workspace
        {
            Name = WorkspaceName,
            Type = string.IsNullOrWhiteSpace(WorkspaceType) ? "General" : WorkspaceType,
            Mode = _isLocalMode ? FlowDesk.Core.Enums.WorkspaceMode.Local : FlowDesk.Core.Enums.WorkspaceMode.Private
        };
        db.Workspaces.Add(workspace);

        var user = new FlowDesk.Core.Models.LocalUser
        {
            Name = string.IsNullOrWhiteSpace(UserName) ? "Owner" : UserName,
            WorkspaceId = workspace.Id,
            Role = "Owner"
        };
        db.LocalUsers.Add(user);

        db.SaveChanges();

        _mainViewModel.NavigateTo(new MainViewModel(_mainViewModel));
    }

    [RelayCommand]
    private void Cancel()
    {
        _mainViewModel.NavigateTo(new WelcomeViewModel(_mainViewModel));
    }
}
