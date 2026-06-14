using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace FlowDesk.Desktop.ViewModels;

public partial class JoinWorkspaceViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainViewModel;
    private readonly FlowDesk.Infrastructure.Services.SignalRClientService _signalRService;

    [ObservableProperty]
    private string _hostUrl = string.Empty;

    [ObservableProperty]
    private string _joinCode = string.Empty;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    private readonly FlowDesk.Infrastructure.Services.NetworkDiscoveryService _discoveryService;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<FlowDesk.Core.Models.DiscoveredHost> _discoveredWorkspaces = new();

    public System.Collections.Generic.IEnumerable<FlowDesk.Core.Models.DiscoveredHost> FilteredWorkspaces => 
        string.IsNullOrWhiteSpace(SearchQuery) 
            ? DiscoveredWorkspaces 
            : System.Linq.Enumerable.Where(DiscoveredWorkspaces, w => w.WorkspaceName.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase) || w.HostUrl.Contains(SearchQuery, System.StringComparison.OrdinalIgnoreCase));

    partial void OnSearchQueryChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredWorkspaces));
    }

    public JoinWorkspaceViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        _signalRService = new FlowDesk.Infrastructure.Services.SignalRClientService();
        _discoveryService = new FlowDesk.Infrastructure.Services.NetworkDiscoveryService();

        _signalRService.OnConnectionStateChanged += state =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => StatusMessage = state);
        };

        _signalRService.OnJoinApproved += state =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => await HandleJoinApprovedAsync());
        };

        _signalRService.OnJoinRejected += state =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => 
            {
                IsLoading = false;
                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                    new FlowDesk.Desktop.Messages.ToastNotificationMessage("Join Request Rejected by Host."));
            });
        };

        var settings = new FlowDesk.Infrastructure.Services.LocalSettingsService().LoadSettings();
        if (!string.IsNullOrWhiteSpace(settings.LastUserName))
        {
            UserName = settings.LastUserName;
        }

        StartDiscovery();
    }

    private void StartDiscovery()
    {
        _discoveryService.StartListening(host =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var existing = System.Linq.Enumerable.FirstOrDefault(DiscoveredWorkspaces, h => h.HostUrl == host.HostUrl);
                if (existing != null)
                {
                    existing.LastSeen = host.LastSeen;
                    existing.WorkspaceName = host.WorkspaceName;
                }
                else
                {
                    DiscoveredWorkspaces.Add(host);
                }
                OnPropertyChanged(nameof(FilteredWorkspaces));
            });
        });
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task JoinWorkspaceAsync(FlowDesk.Core.Models.DiscoveredHost host)
    {
        HostUrl = host.HostUrl;
        JoinCode = host.JoinCode;
        await ConnectAsync();
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConnectAsync()
    {
        if (string.IsNullOrWhiteSpace(HostUrl) || string.IsNullOrWhiteSpace(UserName))
        {
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                new FlowDesk.Desktop.Messages.ToastNotificationMessage("Please fill Host URL and Your Name."));
            return;
        }

        IsLoading = true;
        StatusMessage = "Connecting to Host...";

        try
        {
            var hostIp = HostUrl.Trim();
            if (!hostIp.StartsWith("http")) hostIp = "http://" + hostIp;
            var builder = new System.UriBuilder(hostIp);
            if (builder.Port == 80 || builder.Port == -1)
            {
                builder.Port = 5050;
            }
            var finalUrl = builder.ToString().TrimEnd('/');
            
            // Save final URL so it connects correctly
            HostUrl = finalUrl;

            // Save user name
            var settingsService = new FlowDesk.Infrastructure.Services.LocalSettingsService();
            var settings = settingsService.LoadSettings();
            settings.LastUserName = UserName;
            settingsService.SaveSettings(settings);

            await _signalRService.ConnectAsync(finalUrl);
            StatusMessage = "Requesting to Join...";
            await _signalRService.RequestJoinAsync(UserName, JoinCode);
        }
        catch (System.Exception ex)
        {
            IsLoading = false;
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                new FlowDesk.Desktop.Messages.ToastNotificationMessage($"Failed to connect: {ex.Message}"));
        }
    }

    private async System.Threading.Tasks.Task HandleJoinApprovedAsync()
    {
        try
        {
            IsLoading = false;
            _discoveryService.StopListening();
            
            // Fetch actual workspace name from Host API
            var workspaceName = "Joined Workspace";
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", JoinCode);
                var response = await client.GetAsync($"{HostUrl}/api/workspace");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var doc = System.Text.Json.JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("name", out var nameProp))
                    {
                        workspaceName = nameProp.GetString() ?? workspaceName;
                    }
                    else if (doc.RootElement.TryGetProperty("Name", out var namePropUpper))
                    {
                        workspaceName = namePropUpper.GetString() ?? workspaceName;
                    }
                }
            }
            catch { /* Fallback to default */ }

            // Switch DataSource to Remote
            FlowDesk.Desktop.Services.DataSourceProvider.Initialize("Joined", HostUrl, JoinCode);

            // Save local configuration
            using var db = new FlowDesk.Infrastructure.Data.FlowDeskDbContext();
            
            // Delete old workspace configs
            db.Workspaces.RemoveRange(System.Linq.Enumerable.ToList(db.Workspaces));
            db.LocalUsers.RemoveRange(System.Linq.Enumerable.ToList(db.LocalUsers));

            var ws = new FlowDesk.Core.Models.Workspace
            {
                Name = workspaceName,
                Type = "General",
                Mode = FlowDesk.Core.Enums.WorkspaceMode.Joined,
                HostUrl = HostUrl,
                JoinCode = JoinCode
            };
            db.Workspaces.Add(ws);

            var user = new FlowDesk.Core.Models.LocalUser
            {
                Name = UserName,
                WorkspaceId = ws.Id,
                Role = "Member"
            };
            db.LocalUsers.Add(user);

            db.SaveChanges();

            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                new FlowDesk.Desktop.Messages.ToastNotificationMessage("Successfully joined workspace!"));

            _mainViewModel.NavigateTo(new MainViewModel(_mainViewModel));
        }
        catch (System.Exception ex)
        {
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                new FlowDesk.Desktop.Messages.ToastNotificationMessage($"Critical error joining: {ex.Message}"));
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _discoveryService.StopListening();
        _ = _signalRService.DisconnectAsync();
        _mainViewModel.NavigateTo(new LocalWorkspaceSelectionViewModel(_mainViewModel));
    }
}
