using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace FlowDesk.Infrastructure.Services;

public class SignalRClientService
{
    private HubConnection? _hubConnection;
    public string ConnectionState => _hubConnection?.State.ToString() ?? "Disconnected";

    public event Action<string>? OnConnectionStateChanged;
    public event Action<string>? OnJoinApproved;
    public event Action<string>? OnJoinRejected;
    public event Action? OnHostDisconnected;

    public async Task ConnectAsync(string serverUrl)
    {
        if (_hubConnection != null)
        {
            await DisconnectAsync();
        }

        var hubUrl = $"{serverUrl.TrimEnd('/')}/hubs/workspace";
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.ServerTimeout = TimeSpan.FromSeconds(5);
        _hubConnection.KeepAliveInterval = TimeSpan.FromSeconds(2);

        _hubConnection.Closed += (error) =>
        {
            OnConnectionStateChanged?.Invoke("Disconnected");
            OnHostDisconnected?.Invoke();
            return Task.CompletedTask;
        };

        _hubConnection.Reconnecting += error =>
        {
            OnConnectionStateChanged?.Invoke("Reconnecting");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            OnConnectionStateChanged?.Invoke("Connected");
            return Task.CompletedTask;
        };

        _hubConnection.On("JoinApproved", () =>
        {
            OnJoinApproved?.Invoke("Approved");
        });

        _hubConnection.On("JoinRejected", () =>
        {
            OnJoinRejected?.Invoke("Rejected");
        });

        OnConnectionStateChanged?.Invoke("Connecting");
        await _hubConnection.StartAsync();
        OnConnectionStateChanged?.Invoke("Connected");
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            OnConnectionStateChanged?.Invoke("Disconnected");
        }
    }

    public async Task RequestJoinAsync(string userName, string joinCode)
    {
        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("RequestJoin", userName, joinCode);
        }
    }
}
