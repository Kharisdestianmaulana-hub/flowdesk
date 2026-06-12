using System;
using FlowDesk.Core.Enums;

namespace FlowDesk.Core.Models;

public class Workspace
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public WorkspaceMode Mode { get; set; } = WorkspaceMode.Private;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
