using System;
using FlowDesk.Core.Enums;

namespace FlowDesk.Core.Models;

public class RequestItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public RequestType Type { get; set; } = RequestType.Feature;
    public RequestPriority Priority { get; set; } = RequestPriority.Medium;
    public RequestStatus Status { get; set; } = RequestStatus.Open;
    
    public string RequesterName { get; set; } = string.Empty;
    
    public Guid? ProjectId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
