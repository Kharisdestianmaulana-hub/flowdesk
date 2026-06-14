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

        _hubConnection.Closed += async (error) =>
        {
            OnConnectionStateChanged?.Invoke("Disconnected");
            await Task.Delay(new Random().Next(0, 5) * 1000);
            try { await _hubConnection.StartAsync(); } catch { }
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
