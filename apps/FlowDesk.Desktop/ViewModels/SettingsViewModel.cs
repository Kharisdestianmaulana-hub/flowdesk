using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FlowDesk.Infrastructure.Services;
using System.Diagnostics;
using System;
using System.IO;

namespace FlowDesk.Desktop.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly WorkspaceService _workspaceService = new();
    private readonly FileService _fileService = new();
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private string _workspaceName = string.Empty;

    [ObservableProperty]
    private string _userName = string.Empty;

    public string DataFolderLocation => _fileService.GetDataFolder();

    public SettingsViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        
        var ws = _workspaceService.GetCurrentWorkspace();
        var user = _workspaceService.GetCurrentUser();

        if (ws != null) WorkspaceName = ws.Name;
        if (user != null) UserName = user.Name;
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
    private void CreateManualBackup()
    {
        try
        {
            var backupService = new BackupService();
            backupService.PerformManualBackup();
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Backup created successfully."));
        }
        catch (Exception ex)
        {
            new ExceptionLogger().LogException(ex);
        }
    }
}
