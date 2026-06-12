using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;

namespace FlowDesk.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    [ObservableProperty]
    private ViewModelBase _currentView = default!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHomeActive))]
    [NotifyPropertyChangedFor(nameof(IsProjectsActive))]
    [NotifyPropertyChangedFor(nameof(IsTasksActive))]
    [NotifyPropertyChangedFor(nameof(IsDocsActive))]
    [NotifyPropertyChangedFor(nameof(IsFilesActive))]
    [NotifyPropertyChangedFor(nameof(IsRequestsActive))]
    [NotifyPropertyChangedFor(nameof(IsMembersActive))]
    [NotifyPropertyChangedFor(nameof(IsSettingsActive))]
    private string _activeViewName = "Home";

    [ObservableProperty]
    private string _workspaceName = "My Studio";

    [ObservableProperty]
    private string _userName = "User";

    public bool IsHomeActive => ActiveViewName == "Home";
    public bool IsProjectsActive => ActiveViewName == "Projects";
    public bool IsTasksActive => ActiveViewName == "Tasks";
    public bool IsDocsActive => ActiveViewName == "Docs";
    public bool IsFilesActive => ActiveViewName == "Files";
    public bool IsRequestsActive => ActiveViewName == "Requests";
    public bool IsMembersActive => ActiveViewName == "Members";
    public bool IsSettingsActive => ActiveViewName == "Settings";

    public MainViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

        var workspaceService = new FlowDesk.Infrastructure.Services.WorkspaceService();
        var workspace = workspaceService.GetCurrentWorkspace();
        var user = workspaceService.GetCurrentUser();

        if (workspace != null) WorkspaceName = workspace.Name;
        if (user != null) UserName = user.Name;

        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<FlowDesk.Desktop.Messages.ToastNotificationMessage>(this, (r, m) =>
        {
            ShowToast(m.Value);
        });

        CurrentView = new DashboardViewModel();
    }

    [RelayCommand]
    private void Navigate(string destination)
    {
        ActiveViewName = destination;
        CurrentView = destination switch
        {
            "Home" => new DashboardViewModel(),
            "Projects" => new ProjectsViewModel(this),
            "Tasks" => new TasksViewModel(),
            "Docs" => new DocsViewModel(),
            "Files" => new FilesViewModel(),
            "Requests" => new RequestsViewModel(),
            "Members" => new MembersViewModel(),
            "Settings" => new SettingsViewModel(this),
            _ => new DashboardViewModel()
        };
    }

    public void ChangeView(ViewModelBase newView)
    {
        CurrentView = newView;
    }

    [ObservableProperty] private string _toastMessage = string.Empty;
    [ObservableProperty] private bool _isToastVisible;
    private System.Timers.Timer? _toastTimer;

    public void ShowToast(string message)
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            ToastMessage = message;
            IsToastVisible = true;

            _toastTimer?.Stop();
            _toastTimer = new System.Timers.Timer(3000);
            _toastTimer.AutoReset = false;
            _toastTimer.Elapsed += (s, e) =>
            {
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    IsToastVisible = false;
                });
            };
            _toastTimer.Start();
        });
    }

    [RelayCommand]
    private void CloseToast()
    {
        IsToastVisible = false;
        _toastTimer?.Stop();
    }
}
