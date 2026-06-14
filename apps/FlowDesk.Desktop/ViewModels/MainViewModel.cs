using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Linq;
using Microsoft.AspNetCore.SignalR;

namespace FlowDesk.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainWindowViewModel MainWindowViewModel { get; }

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
    private bool _isSidebarOpen = true;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public OmnibarViewModel Omnibar { get; } = new();

    [ObservableProperty]
    private string _workspaceName = "My Studio";

    [ObservableProperty]
    private string _workspaceModeName = "Private Workspace";

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

    public MainViewModel(MainWindowViewModel mainWindowViewModel, FlowDesk.Infrastructure.Services.SignalRClientService? signalRService = null)
    {
        MainWindowViewModel = mainWindowViewModel;

        var workspaceService = new FlowDesk.Infrastructure.Services.WorkspaceService();
        var workspace = workspaceService.GetCurrentWorkspace();
        var user = workspaceService.GetCurrentUser();

        if (workspace != null) 
        {
            WorkspaceName = workspace.Name;
            WorkspaceModeName = workspace.Mode switch
            {
                FlowDesk.Core.Enums.WorkspaceMode.Local => "Local Workspace",
                FlowDesk.Core.Enums.WorkspaceMode.Private => "Private Workspace",
                FlowDesk.Core.Enums.WorkspaceMode.Joined => "Joined Workspace",
                _ => "Workspace"
            };
        }
        if (user != null) UserName = user.Name;

        if (signalRService != null)
        {
            signalRService.OnHostDisconnected += () =>
            {
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                        new FlowDesk.Desktop.Messages.ToastNotificationMessage("Host disconnected. Redirecting to welcome screen..."));
                    
                    await System.Threading.Tasks.Task.Delay(3000);

                    using var db = new FlowDesk.Infrastructure.Data.FlowDeskDbContext();
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    
                    MainWindowViewModel.NavigateTo(new WelcomeViewModel(MainWindowViewModel));
                });
            };
        }

        Omnibar.OnResultSelected = HandleOmnibarResult;

        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<FlowDesk.Desktop.Messages.ToastNotificationMessage>(this, (r, m) =>
        {
            ShowToast(m.Value);
        });

        PendingJoinRequests.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasPendingJoinRequests));

        FlowDesk.Infrastructure.Hosting.WorkspaceHub.OnJoinRequestReceived += message =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                PendingJoinRequests.Add(message);
                ShowToast($"New join request from {message.UserName}");
            });
        };

        FlowDesk.Infrastructure.Hosting.WorkspaceHub.OnMemberDisconnected += userName =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                using var db = new FlowDesk.Infrastructure.Data.FlowDeskDbContext();
                var userToRemove = db.LocalUsers.FirstOrDefault(u => u.Name == userName && u.Role != "Owner");
                if (userToRemove != null)
                {
                    db.LocalUsers.Remove(userToRemove);
                    await db.SaveChangesAsync();
                    
                    // Reload members if Members view is active
                    if (CurrentView is MembersViewModel mvm)
                    {
                        mvm.LoadMembersAsync().GetAwaiter().GetResult();
                    }
                }
                ShowToast($"{userName} disconnected from the workspace.");
            });
        };

        CurrentView = new DashboardViewModel();
    }

    public System.Collections.ObjectModel.ObservableCollection<FlowDesk.Infrastructure.Hosting.JoinRequestMessage> PendingJoinRequests { get; } = new();
    
    public bool HasPendingJoinRequests => PendingJoinRequests.Count > 0;
    
    [ObservableProperty] private bool _isJoinRequestOpen; // Kept for backwards compatibility if referenced, but unused

    public async System.Threading.Tasks.Task ApproveJoinAsync(FlowDesk.Infrastructure.Hosting.JoinRequestMessage request)
    {
        PendingJoinRequests.Remove(request);
        var hub = FlowDesk.Infrastructure.Hosting.LocalServerHost.HubContext;
        if (hub != null)
        {
            await hub.Clients.Client(request.ConnectionId).SendAsync("JoinApproved");
            
            // Track the connection so we can detect disconnects
            FlowDesk.Infrastructure.Hosting.WorkspaceHub.RegisterMemberConnection(request.ConnectionId, request.UserName);

            // Save member to database
            using var db = new FlowDesk.Infrastructure.Data.FlowDeskDbContext();
            var workspace = db.Workspaces.FirstOrDefault();
            if (workspace != null)
            {
                var existingUser = db.LocalUsers.FirstOrDefault(u => u.Name == request.UserName);
                if (existingUser == null)
                {
                    db.LocalUsers.Add(new FlowDesk.Core.Models.LocalUser 
                    { 
                        Name = request.UserName, 
                        Role = "Join", 
                        WorkspaceId = workspace.Id 
                    });
                    await db.SaveChangesAsync();
                }
            }

            ShowToast($"{request.UserName} joined the workspace.");
        }
    }

    public async System.Threading.Tasks.Task RejectJoinAsync(FlowDesk.Infrastructure.Hosting.JoinRequestMessage request)
    {
        PendingJoinRequests.Remove(request);
        var hub = FlowDesk.Infrastructure.Hosting.LocalServerHost.HubContext;
        if (hub != null)
        {
            await hub.Clients.Client(request.ConnectionId).SendAsync("JoinRejected");
        }
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
            "Members" => new MembersViewModel(this),
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

    [RelayCommand]
    private void ExecuteNew()
    {
        if (CurrentView is IPageCommands page && page.NewCommand?.CanExecute(null) == true)
        {
            page.NewCommand.Execute(null);
        }
    }

    [RelayCommand]
    private void ExecuteSave()
    {
        if (CurrentView is IPageCommands page && page.SaveCommand?.CanExecute(null) == true)
        {
            page.SaveCommand.Execute(null);
        }
    }

    [RelayCommand]
    private void ExecuteSearch()
    {
        if (CurrentView is IPageCommands page && page.SearchCommand?.CanExecute(null) == true)
        {
            page.SearchCommand.Execute(null);
        }
    }

    [RelayCommand]
    public void ToggleOmnibar()
    {
        if (Omnibar.IsOpen)
            Omnibar.Close();
        else
            Omnibar.Open();
    }

    private void HandleOmnibarResult(Models.SearchResultItem item)
    {
        switch (item.Type)
        {
            case "Project":
                Navigate("Projects");
                break;
            case "Task":
                Navigate("Tasks");
                break;
            case "Doc":
                Navigate("Docs");
                break;
            case "File":
                Navigate("Files");
                break;
        }
    }

    [RelayCommand]
    private void ExecuteClose()
    {
        if (CurrentView is IPageCommands page && page.CloseCommand?.CanExecute(null) == true)
        {
            page.CloseCommand.Execute(null);
        }
    }
}
