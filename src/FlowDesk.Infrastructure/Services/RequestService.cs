using FlowDesk.Core.Enums;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowDesk.Infrastructure.Services;

public class RequestService
{
    private readonly ActivityLogService _logService = new();

    public List<RequestItem> GetRequests()
    {
        using var db = new FlowDeskDbContext();
        return db.Requests.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public List<RequestItem> GetRequestsForProject(Guid projectId)
    {
        using var db = new FlowDeskDbContext();
        return db.Requests.Where(r => r.ProjectId == projectId).OrderByDescending(r => r.CreatedAt).ToList();
    }

    public RequestItem CreateRequest(string title, string? description, RequestType type, RequestPriority priority, string requesterName, Guid? projectId)
    {
        using var db = new FlowDeskDbContext();
        var user = db.LocalUsers.FirstOrDefault();
        var actorName = user?.Name ?? "System";

        var req = new RequestItem
        {
            Title = title,
            Description = description,
            Type = type,
            Priority = priority,
            Status = RequestStatus.Open,
            RequesterName = string.IsNullOrWhiteSpace(requesterName) ? actorName : requesterName,
            ProjectId = projectId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Requests.Add(req);
        db.SaveChanges();

        _logService.LogActivity(actorName, "Created", "Request", req.Id);

        return req;
    }

    public RequestItem? UpdateRequest(Guid id, string title, string? description, RequestType type, RequestPriority priority, RequestStatus status, string requesterName, Guid? projectId)
    {
        using var db = new FlowDeskDbContext();
        var req = db.Requests.Find(id);
        if (req == null) return null;

        req.Title = title;
        req.Description = description;
        req.Type = type;
        req.Priority = priority;
        req.Status = status;
        req.RequesterName = requesterName;
        req.ProjectId = projectId;
        req.UpdatedAt = DateTime.UtcNow;

        db.SaveChanges();

        var user = db.LocalUsers.FirstOrDefault();
        var actorName = user?.Name ?? "System";
        _logService.LogActivity(actorName, "Updated", "Request", req.Id);

        return req;
    }

    public void DeleteRequest(Guid id)
    {
        using var db = new FlowDeskDbContext();
        var req = db.Requests.Find(id);
        if (req != null)
        {
            db.Requests.Remove(req);
            db.SaveChanges();

            var user = db.LocalUsers.FirstOrDefault();
            var actorName = user?.Name ?? "System";
            _logService.LogActivity(actorName, "Deleted", "Request", id);
        }
    }
}
