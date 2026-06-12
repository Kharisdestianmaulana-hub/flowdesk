using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Data;
using System;
using System.Linq;

namespace FlowDesk.Infrastructure.Services;

public class ActivityLogService
{
    public void LogActivity(string actorName, string action, string targetType, Guid targetId)
    {
        using var db = new FlowDeskDbContext();
        var workspaceId = db.Workspaces.FirstOrDefault()?.Id ?? Guid.Empty;

        var log = new ActivityLog
        {
            WorkspaceId = workspaceId,
            ActorName = actorName,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            CreatedAt = DateTime.UtcNow
        };
        db.ActivityLogs.Add(log);
        db.SaveChanges();
    }

    public System.Collections.Generic.List<ActivityLog> GetLogsForEntity(Guid targetId)
    {
        using var db = new FlowDeskDbContext();
        return db.ActivityLogs.Where(a => a.TargetId == targetId).OrderByDescending(a => a.CreatedAt).ToList();
    }
}
