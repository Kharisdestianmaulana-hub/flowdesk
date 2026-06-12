using FlowDesk.Core.Enums;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowDesk.Infrastructure.Services;

public class ProjectService
{
    private readonly ActivityLogService _logService = new();

    public List<Project> GetProjects()
    {
        using var db = new FlowDeskDbContext();
        return db.Projects.OrderByDescending(p => p.CreatedAt).ToList();
    }

    public Project CreateProject(string name, string? description, ProjectStatus status, ProjectType type)
    {
        using var db = new FlowDeskDbContext();
        var workspaceId = db.Workspaces.FirstOrDefault()?.Id ?? Guid.Empty;
        var user = db.LocalUsers.FirstOrDefault();
        var actorName = user?.Name ?? "System";

        var project = new Project
        {
            WorkspaceId = workspaceId,
            Name = name,
            Description = description,
            Status = status,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Projects.Add(project);
        db.SaveChanges();

        _logService.LogActivity(actorName, "Created", "Project", project.Id);

        return project;
    }

    public Project? UpdateProject(Guid id, string name, string? description, ProjectStatus status, ProjectType type)
    {
        using var db = new FlowDeskDbContext();
        var project = db.Projects.Find(id);
        if (project == null) return null;

        project.Name = name;
        project.Description = description;
        project.Status = status;
        project.Type = type;
        project.UpdatedAt = DateTime.UtcNow;

        db.SaveChanges();

        var user = db.LocalUsers.FirstOrDefault();
        var actorName = user?.Name ?? "System";
        _logService.LogActivity(actorName, "Updated details", "Project", project.Id);

        return project;
    }

    public void DeleteProject(Guid id)
    {
        using var db = new FlowDeskDbContext();
        var project = db.Projects.Find(id);
        if (project != null)
        {
            // Safely unlink all child data instead of cascading delete
            var tasks = db.Tasks.Where(t => t.ProjectId == id).ToList();
            foreach (var t in tasks) t.ProjectId = null;

            var docs = db.Documents.Where(d => d.ProjectId == id).ToList();
            foreach (var d in docs) d.ProjectId = null;

            var files = db.Files.Where(f => f.ProjectId == id).ToList();
            foreach (var f in files) f.ProjectId = null;

            var requests = db.Requests.Where(r => r.ProjectId == id).ToList();
            foreach (var r in requests) r.ProjectId = null;

            db.Projects.Remove(project);
            db.SaveChanges();

            var user = db.LocalUsers.FirstOrDefault();
            var actorName = user?.Name ?? "System";
            _logService.LogActivity(actorName, "Deleted", "Project", id);
        }
    }
}
