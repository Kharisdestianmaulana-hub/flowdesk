using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Data;
using System.Linq;

namespace FlowDesk.Infrastructure.Services;

public class WorkspaceService
{
    public Workspace? GetCurrentWorkspace()
    {
        using var db = new FlowDeskDbContext();
        return db.Workspaces.FirstOrDefault();
    }

    public LocalUser? GetCurrentUser()
    {
        using var db = new FlowDeskDbContext();
        return db.LocalUsers.FirstOrDefault();
    }

    public void UpdateWorkspaceName(string newName)
    {
        using var db = new FlowDeskDbContext();
        var workspace = db.Workspaces.FirstOrDefault();
        if (workspace != null)
        {
            workspace.Name = newName;
            db.SaveChanges();
        }
    }

    public void UpdateUserName(string newName)
    {
        using var db = new FlowDeskDbContext();
        var user = db.LocalUsers.FirstOrDefault();
        if (user != null)
        {
            user.Name = newName;
            db.SaveChanges();
        }
    }
}
