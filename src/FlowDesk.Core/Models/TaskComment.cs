using System;

namespace FlowDesk.Core.Models;

public class TaskComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskId { get; set; }
    
    // Who wrote this comment
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorInitial { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public TaskItem Task { get; set; } = null!;
}
