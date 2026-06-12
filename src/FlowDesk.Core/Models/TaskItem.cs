using System;
using FlowDesk.Core.Enums;

namespace FlowDesk.Core.Models;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public FlowDesk.Core.Enums.TaskStatus Status { get; set; } = FlowDesk.Core.Enums.TaskStatus.Backlog;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid? ProjectId { get; set; }
    public Guid? AssigneeId { get; set; }
}
