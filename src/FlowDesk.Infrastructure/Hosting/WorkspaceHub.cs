using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;

namespace FlowDesk.Infrastructure.Hosting;

public class JoinRequestMessage
{
    public string ConnectionId { get; }
    public string UserName { get; }

    public JoinRequestMessage(string connectionId, string userName)
    {
        ConnectionId = connectionId;
        UserName = userName;
    }
}

public class WorkspaceHub : Hub
{
    public static event Action<JoinRequestMessage>? OnJoinRequestReceived;
    public static event Action<string>? OnMemberDisconnected;

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _connectedMembers = new();

    public static void RegisterMemberConnection(string connectionId, string userName)
    {
        _connectedMembers[connectionId] = userName;
    }

    public async Task RequestJoin(string userName, string joinCode)
    {
        OnJoinRequestReceived?.Invoke(new JoinRequestMessage(Context.ConnectionId, userName));
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectedMembers.TryRemove(Context.ConnectionId, out var userName))
        {
            OnMemberDisconnected?.Invoke(userName);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
