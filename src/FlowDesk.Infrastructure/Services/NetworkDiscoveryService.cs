using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FlowDesk.Core.Models;

namespace FlowDesk.Infrastructure.Services;

public class NetworkDiscoveryService : IDisposable
{
    private const int DiscoveryPort = 5051;
    private UdpClient? _broadcaster;
    private UdpClient? _listener;
    private CancellationTokenSource? _broadcastCts;
    private CancellationTokenSource? _listenCts;

    public void StartBroadcasting(string workspaceName, string hostUrl, string joinCode)
    {
        StopBroadcasting();
        _broadcastCts = new CancellationTokenSource();
        _broadcaster = new UdpClient();
        _broadcaster.EnableBroadcast = true;

        var messageObj = new
        {
            WorkspaceName = workspaceName,
            HostUrl = hostUrl,
            JoinCode = joinCode
        };
        var messageJson = JsonSerializer.Serialize(messageObj);
        var bytes = Encoding.UTF8.GetBytes(messageJson);
        var endpoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);

        Task.Run(async () =>
        {
            while (!_broadcastCts.Token.IsCancellationRequested)
            {
                try
                {
                    await _broadcaster.SendAsync(bytes, bytes.Length, endpoint);
                }
                catch
                {
                    // Ignore broadcast errors (e.g., network disconnected)
                }
                await Task.Delay(2000, _broadcastCts.Token); // Broadcast every 2 seconds
            }
        }, _broadcastCts.Token);
    }

    public void StopBroadcasting()
    {
        if (_broadcastCts != null)
        {
            _broadcastCts.Cancel();
            _broadcastCts.Dispose();
            _broadcastCts = null;
        }

        if (_broadcaster != null)
        {
            _broadcaster.Close();
            _broadcaster.Dispose();
            _broadcaster = null;
        }
    }

    public void StartListening(Action<DiscoveredHost> onHostFound)
    {
        StopListening();
        _listenCts = new CancellationTokenSource();
        
        try
        {
            // Bind to Any IP on the discovery port
            var endpoint = new IPEndPoint(IPAddress.Any, DiscoveryPort);
            _listener = new UdpClient();
            _listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listener.ExclusiveAddressUse = false;
            _listener.Client.Bind(endpoint);

            Task.Run(async () =>
            {
                while (!_listenCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await _listener.ReceiveAsync(_listenCts.Token);
                        var json = Encoding.UTF8.GetString(result.Buffer);
                        var doc = JsonDocument.Parse(json);
                        
                        var host = new DiscoveredHost
                        {
                            WorkspaceName = doc.RootElement.TryGetProperty("WorkspaceName", out var wsName) ? wsName.GetString() ?? "Unknown Workspace" : "Unknown Workspace",
                            HostUrl = doc.RootElement.TryGetProperty("HostUrl", out var url) ? url.GetString() ?? "" : "",
                            JoinCode = doc.RootElement.TryGetProperty("JoinCode", out var code) ? code.GetString() ?? "" : "",
                            LastSeen = DateTime.UtcNow
                        };

                        if (!string.IsNullOrEmpty(host.HostUrl))
                        {
                            onHostFound(host);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        // Ignore parse errors or unknown formats
                    }
                }
            }, _listenCts.Token);
        }
        catch
        {
            // Port might be in use or access denied
        }
    }

    public void StopListening()
    {
        if (_listenCts != null)
        {
            _listenCts.Cancel();
            _listenCts.Dispose();
            _listenCts = null;
        }

        if (_listener != null)
        {
            _listener.Close();
            _listener.Dispose();
            _listener = null;
        }
    }

    public void Dispose()
    {
        StopBroadcasting();
        StopListening();
    }
}
