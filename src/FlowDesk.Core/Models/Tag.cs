using System;
using System.Collections.Generic;

namespace FlowDesk.Core.Models;

public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#808080"; // Default subtle gray
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProjectTag> ProjectTags { get; set; } = new List<ProjectTag>();
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}

public class ProjectTag
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}

public class TaskTag
{
    public Guid TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
