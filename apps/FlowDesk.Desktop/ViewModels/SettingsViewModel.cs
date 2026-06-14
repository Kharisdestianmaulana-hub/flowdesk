using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FlowDesk.Infrastructure.Services;
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using FlowDesk.Infrastructure.Hosting;

namespace FlowDesk.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly WorkspaceService _workspaceService = new();
    private readonly FileService _fileService = new();
    private readonly MainViewModel _mainViewModel;
    private static LocalServerHost? _serverHost;

    [ObservableProperty] private bool _isLocalServerRunning;
    [ObservableProperty] private bool _allowLanAccess;
    [ObservableProperty] private string _localServerUrl = string.Empty;
    [ObservableProperty] private string _joinCode = string.Empty;
    
    [ObservableProperty] private string _joinHostUrl = string.Empty;
    [ObservableProperty] private string _joinHostCode = string.Empty;

    [ObservableProperty]
    private string _workspaceName = string.Empty;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _loadingMessage = string.Empty;

    [ObservableProperty]
    private bool _isLeaveWorkspaceVisible;

    [ObservableProperty]
    private bool _isHostSettingsVisible;

    [ObservableProperty]
    private bool _isLeaveConfirmationVisible;

    [RelayCommand]
    private void LeaveWorkspace()
    {
        IsLeaveConfirmationVisible = true;
    }

    [RelayCommand]
    private void CancelLeaveWorkspace()
    {
        IsLeaveConfirmationVisible = false;
    }

    [RelayCommand]
    private async Task ConfirmLeaveWorkspaceAsync()
    {
        IsLeaveConfirmationVisible = false;
        
        // Delete all data and return to welcome screen
        using var db = new FlowDesk.Infrastructure.Data.FlowDeskDbContext();
        await db.Database.EnsureDeletedAsync();
        
        _mainViewModel.MainWindowViewModel.NavigateTo(new WelcomeViewModel(_mainViewModel.MainWindowViewModel));
    }

    public string DataFolderLocation => _fileService.GetDataFolder();
    public string AppVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.1.0";

    public System.Collections.ObjectModel.ObservableCollection<string> AvailableThemes { get; } = new() { "System", "Light", "Dark" };

    [ObservableProperty]
    private string _selectedTheme = "System";

    public System.Collections.ObjectModel.ObservableCollection<string> AvailableZoomOptions { get; } = new() { "50%", "75%", "100%", "125%", "150%", "200%", "Custom" };

    private string _selectedZoomOption = "100%";
    public string SelectedZoomOption
    {
        get => _selectedZoomOption;
        set
        {
            if (SetProperty(ref _selectedZoomOption, value))
            {
                OnPropertyChanged(nameof(IsCustomZoomVisible));
                if (value != "Custom")
                {
                    if (double.TryParse(value.Replace("%", ""), out double pct))
                    {
                        UpdateAppZoomLevel(pct / 100.0);
                    }
                }
                else
                {
                    CustomZoomText = Math.Round(_mainViewModel.MainWindowViewModel.AppZoomLevel * 100).ToString();
                }
            }
        }
    }

    public bool IsCustomZoomVisible => SelectedZoomOption == "Custom";

    private string _customZoomText = "100";
    public string CustomZoomText
    {
        get => _customZoomText;
        set
        {
            if (SetProperty(ref _customZoomText, value))
            {
                if (double.TryParse(value, out double pct))
                {
                    if (pct < 50) pct = 50;
                    if (pct > 300) pct = 300;
                    UpdateAppZoomLevel(pct / 100.0);
                }
            }
        }
    }

    private void UpdateAppZoomLevel(double value)
    {
        if (_mainViewModel.MainWindowViewModel.AppZoomLevel != value)
        {
            _mainViewModel.MainWindowViewModel.AppZoomLevel = value;
            var settings = _settingsService.LoadSettings();
            settings.AppZoomLevel = value;
            _settingsService.SaveSettings(settings);
        }
    }

    private readonly LocalSettingsService _settingsService = new();

    public SettingsViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        
        var ws = _workspaceService.GetCurrentWorkspace();
        var user = _workspaceService.GetCurrentUser();

        if (ws != null) 
        {
            WorkspaceName = ws.Name;
            JoinCode = ws.JoinCode ?? string.Empty;
        }
        if (user != null) UserName = user.Name;

        if (_serverHost != null)
        {
            IsLocalServerRunning = _serverHost.IsRunning;
            if (IsLocalServerRunning)
            {
                LocalServerUrl = AllowLanAccess ? $"http://{GetLocalIPAddress()}:5050" : "http://localhost:5050";
            }
        }

        IsLeaveWorkspaceVisible = ws?.Mode != FlowDesk.Core.Enums.WorkspaceMode.Local;
        IsHostSettingsVisible = ws?.Mode != FlowDesk.Core.Enums.WorkspaceMode.Joined;

        var settings = _settingsService.LoadSettings();
        SelectedTheme = settings.ThemePreference;

        var zl = Math.Round(_mainViewModel.MainWindowViewModel.AppZoomLevel * 100);
        string mapped = zl switch {
            50 => "50%",
            75 => "75%",
            100 => "100%",
            125 => "125%",
            150 => "150%",
            200 => "200%",
            _ => "Custom"
        };
        
        _selectedZoomOption = mapped;
        if (mapped == "Custom") {
            _customZoomText = zl.ToString();
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        if (!string.IsNullOrWhiteSpace(WorkspaceName))
        {
            _workspaceService.UpdateWorkspaceName(WorkspaceName);
            _mainViewModel.WorkspaceName = WorkspaceName;
        }
            
        if (!string.IsNullOrWhiteSpace(UserName))
        {
            _workspaceService.UpdateUserName(UserName);
            _mainViewModel.UserName = UserName;
        }

        var settings = _settingsService.LoadSettings();
        settings.ThemePreference = SelectedTheme;
        _settingsService.SaveSettings(settings);

        // Apply theme immediately
        if (Avalonia.Application.Current != null)
        {
            Avalonia.Application.Current.RequestedThemeVariant = SelectedTheme switch
            {
                "Light" => Avalonia.Styling.ThemeVariant.Light,
                "Dark" => Avalonia.Styling.ThemeVariant.Dark,
                _ => Avalonia.Styling.ThemeVariant.Default
            };
        }

        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Settings saved."));
    }

    [RelayCommand]
    private void OpenDataFolder()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var flowDeskFolder = Path.Join(appData, "FlowDeskData");
        
        if (Directory.Exists(flowDeskFolder))
        {
            Process.Start(new ProcessStartInfo("open", $"\"{flowDeskFolder}\"") { UseShellExecute = true });
        }
    }

    [RelayCommand]
    private void OpenBackupFolder()
    {
        var backupService = new BackupService();
        var backupFolder = backupService.GetBackupFolder();
        
        if (Directory.Exists(backupFolder))
        {
            Process.Start(new ProcessStartInfo("open", $"\"{backupFolder}\"") { UseShellExecute = true });
        }
    }

    [RelayCommand]
    private async Task CreateManualBackupAsync()
    {
        try
        {
            IsLoading = true;
            LoadingMessage = "Creating backup...";
            var backupService = new BackupService();
            await Task.Run(() => backupService.PerformManualBackup());
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Backup created successfully."));
        }
        catch (Exception ex)
        {
            new ExceptionLogger().LogException(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }
    [RelayCommand]
    private async Task ExportWorkspaceAsync()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            if (topLevel == null) return;

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd");
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export Workspace",
                SuggestedFileName = $"FlowDesk-Workspace-Backup-{timestamp}.zip",
                DefaultExtension = "zip",
                ShowOverwritePrompt = true
            });

            if (file != null)
            {
                try
                {
                    IsLoading = true;
                    LoadingMessage = "Exporting workspace...";
                    var backupService = new BackupService();
                    await Task.Run(() => backupService.ExportWorkspaceToZip(file.Path.LocalPath));
                    CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage($"Workspace exported successfully.\nSaved to: {file.Path.LocalPath}"));
                }
                catch (Exception ex)
                {
                    new ExceptionLogger().LogException(ex);
                    CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Failed to export workspace."));
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }

    private readonly FlowDesk.Infrastructure.Services.NetworkDiscoveryService _discoveryService = new();

    [RelayCommand]
    private async System.Threading.Tasks.Task ToggleServerAsync()
    {
        if (IsLocalServerRunning)
        {
            if (_serverHost != null)
            {
                await _serverHost.StopAsync();
                _discoveryService.StopBroadcasting();
                _serverHost = null;
                IsLocalServerRunning = false;
                LocalServerUrl = string.Empty;
                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                    new FlowDesk.Desktop.Messages.ToastNotificationMessage("Local server stopped."));
            }
            return;
        }

        try
        {
            var ws = _workspaceService.GetCurrentWorkspace();
            _serverHost = new FlowDesk.Infrastructure.Hosting.LocalServerHost();
            await _serverHost.StartAsync(AllowLanAccess);
            IsLocalServerRunning = true;
            
            if (AllowLanAccess)
            {
                LocalServerUrl = $"http://{GetLocalIPAddress()}:5050";
                _discoveryService.StartBroadcasting(ws?.Name ?? "FlowDesk Workspace", LocalServerUrl, ws?.JoinCode ?? string.Empty);
            }
            else
            {
                LocalServerUrl = "http://localhost:5050";
                _discoveryService.StartBroadcasting(ws?.Name ?? "FlowDesk Workspace", LocalServerUrl, ws?.JoinCode ?? string.Empty);
            }
        }
        catch (Exception ex)
        {
            new ExceptionLogger().LogException(ex);
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                new FlowDesk.Desktop.Messages.ToastNotificationMessage($"Failed to start server: {ex.Message}"));
            IsLocalServerRunning = false;
        }
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }

    [RelayCommand]
    private async Task ConnectToWorkspaceAsync()
    {
        if (string.IsNullOrWhiteSpace(JoinHostUrl)) return;
        
        IsLoading = true;
        LoadingMessage = "Connecting...";

        try
        {
            var hostIp = JoinHostUrl.Trim();
            if (!hostIp.StartsWith("http")) hostIp = "http://" + hostIp;
            var builder = new System.UriBuilder(hostIp);
            if (builder.Port == 80 || builder.Port == -1)
            {
                builder.Port = 5050;
            }
            var finalUrl = builder.ToString().TrimEnd('/');
            
            // Save formatted url
            JoinHostUrl = finalUrl;

            using var client = new System.Net.Http.HttpClient();
            var url = $"{finalUrl}/api/workspace?code={JoinHostCode}";
            var response = await client.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage($"Connected to remote workspace!\n{content}"));
            }
            else
            {
                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage($"Failed to connect: {response.StatusCode}"));
            }
        }
        catch (Exception ex)
        {
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage($"Connection error: {ex.Message}"));
        }
        finally
        {
            IsLoading = false;
        }
    }
}
