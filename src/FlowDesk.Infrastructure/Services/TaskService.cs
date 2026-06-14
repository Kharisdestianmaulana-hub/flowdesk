using FlowDesk.Core.Enums;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowDesk.Infrastructure.Services;

public class TaskService
{
    private readonly ActivityLogService _logService = new();

    public List<TaskItem> GetTasks()
    {
        using var db = new FlowDeskDbContext();
        return db.Tasks.OrderByDescending(t => t.CreatedAt).ToList();
    }

    public TaskItem CreateTask(string title, Guid? projectId, FlowDesk.Core.Enums.TaskStatus status, TaskPriority priority, DateTime? dueDate = null)
    {
        using var db = new FlowDeskDbContext();
        var user = db.LocalUsers.FirstOrDefault();
        var actorName = user?.Name ?? "System";

        var task = new TaskItem
        {
            Title = title,
            ProjectId = projectId,
            Status = status,
            Priority = priority,
            DueDate = dueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Tasks.Add(task);
        db.SaveChanges();

        _logService.LogActivity(actorName, "Created", "Task", task.Id);

        return task;
    }

    public TaskItem? UpdateTaskStatus(Guid id, FlowDesk.Core.Enums.TaskStatus newStatus)
    {
        using var db = new FlowDeskDbContext();
        var task = db.Tasks.Find(id);
        if (task == null) return null;

        var oldStatus = task.Status;
        if (oldStatus == newStatus) return task;

        task.Status = newStatus;
        task.UpdatedAt = DateTime.UtcNow;
        db.SaveChanges();

        var user = db.LocalUsers.FirstOrDefault();
        var actorName = user?.Name ?? "System";
        _logService.LogActivity(actorName, $"Status Changed to {newStatus}", "Task", task.Id);

        return task;
    }

    public TaskItem? UpdateTask(Guid id, string title, string? description, FlowDesk.Core.Enums.TaskStatus status, TaskPriority priority, DateTime? dueDate, Guid? projectId)
    {
        using var db = new FlowDeskDbContext();
        var task = db.Tasks.Find(id);
        if (task == null) return null;

        task.Title = title;
        task.Description = description;
        task.Status = status;
        task.Priority = priority;
        task.DueDate = dueDate;
        task.ProjectId = projectId;
        task.UpdatedAt = DateTime.UtcNow;
        
        db.SaveChanges();

        var user = db.LocalUsers.FirstOrDefault();
        var actorName = user?.Name ?? "System";
        _logService.LogActivity(actorName, "Updated details", "Task", task.Id);

        return task;
    }

    public void DeleteTask(Guid id)
    {
        using var db = new FlowDeskDbContext();
        var task = db.Tasks.Find(id);
        if (task != null)
        {
            db.Tasks.Remove(task);
            db.SaveChanges();

            var user = db.LocalUsers.FirstOrDefault();
            var actorName = user?.Name ?? "System";
            _logService.LogActivity(actorName, "Deleted", "Task", id);
        }
    }
}
