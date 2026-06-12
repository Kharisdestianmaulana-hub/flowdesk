using System;

namespace FlowDesk.Core.Models;

public class FileItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string OriginalPath { get; set; } = string.Empty;
    public string StoredPath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Extension { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
