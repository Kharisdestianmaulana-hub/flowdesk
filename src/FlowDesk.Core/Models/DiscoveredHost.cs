using System;

namespace FlowDesk.Core.Models;

public class DiscoveredHost
{
    public string WorkspaceName { get; set; } = string.Empty;
    public string HostUrl { get; set; } = string.Empty;
    public string JoinCode { get; set; } = string.Empty;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
}
