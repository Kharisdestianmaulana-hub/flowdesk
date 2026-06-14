using CommunityToolkit.Mvvm.ComponentModel;
using FlowDesk.Desktop.Services;
using FlowDesk.Core.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace FlowDesk.Desktop.ViewModels;

public partial class MembersViewModel : ViewModelBase
{
    private readonly FlowDesk.Infrastructure.Services.WorkspaceService _workspaceService = new();

    [ObservableProperty]
    private string _workspaceModeName = "Private";

    [ObservableProperty]
    private string _infoNote = "In Private Workspace mode, you are the sole member and owner.";

    public ObservableCollection<LocalUser> Members { get; } = new();

    private readonly MainViewModel _mainViewModel;
    public MainViewModel MainViewModel => _mainViewModel;

    [ObservableProperty]
    private bool _isHost;

    public MembersViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        var workspace = _workspaceService.GetCurrentWorkspace();
        if (workspace != null)
        {
            WorkspaceModeName = workspace.Mode == FlowDesk.Core.Enums.WorkspaceMode.Local ? "Local" : workspace.Mode == FlowDesk.Core.Enums.WorkspaceMode.Joined ? "Joined" : "Private";
            IsHost = workspace.Mode != FlowDesk.Core.Enums.WorkspaceMode.Joined;
            InfoNote = workspace.Mode == FlowDesk.Core.Enums.WorkspaceMode.Local 
                ? "In Local Workspace mode, you can manage who joins your team."
                : workspace.Mode == FlowDesk.Core.Enums.WorkspaceMode.Joined 
                ? "You are connected to a remote workspace."
                : "In Private Workspace mode, you are the sole member and owner.";
        }

        _ = LoadMembersAsync();
    }

    public async Task LoadMembersAsync()
    {
        var members = await DataSourceProvider.Current.GetMembersAsync();
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Members.Clear();
            foreach (var m in members)
            {
                Members.Add(m);
            }
        });
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private async Task ApproveJoinAsync(FlowDesk.Infrastructure.Hosting.JoinRequestMessage request)
    {
        await _mainViewModel.ApproveJoinAsync(request);
        await LoadMembersAsync(); // reload members
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private async Task RejectJoinAsync(FlowDesk.Infrastructure.Hosting.JoinRequestMessage request)
    {
        await _mainViewModel.RejectJoinAsync(request);
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private async Task KickMemberAsync(FlowDesk.Core.Models.LocalUser user)
    {
        if (user.Role == "Owner") return;

        using var db = new FlowDesk.Infrastructure.Data.FlowDeskDbContext();
        var entity = db.LocalUsers.FirstOrDefault(u => u.Id == user.Id);
        if (entity != null)
        {
            db.LocalUsers.Remove(entity);
            await db.SaveChangesAsync();
            _mainViewModel.ShowToast($"{user.Name} has been removed from the workspace.");
            await LoadMembersAsync();
        }
    }
}
