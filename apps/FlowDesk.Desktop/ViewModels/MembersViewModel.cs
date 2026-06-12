using CommunityToolkit.Mvvm.ComponentModel;
using FlowDesk.Infrastructure.Services;

namespace FlowDesk.Desktop.ViewModels;

public partial class MembersViewModel : ViewModelBase
{
    private readonly WorkspaceService _workspaceService = new();

    [ObservableProperty]
    private string _userName = "User";

    public MembersViewModel()
    {
        var user = _workspaceService.GetCurrentUser();
        if (user != null)
        {
            UserName = user.Name;
        }
    }
}
