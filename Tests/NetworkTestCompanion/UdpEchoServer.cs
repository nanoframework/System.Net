// Copyright (c) .NET Foundation and Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Sockets;

namespace NetworkTestCompanion;

/// <summary>
/// Listens on a UDP port and echoes each received datagram back to its sender.
/// </summary>
internal sealed class UdpEchoServer : IDisposable
{
    private readonly UdpClient _udpClient;
    private readonly CancellationTokenSource _cts = new();
    private Task? _receiveLoop;

    internal int Port { get; }

    internal UdpEchoServer(IPAddress bindAddress, int port)
    {
        Port = port;
        _udpClient = new UdpClient(new IPEndPoint(bindAddress, port));
    }

    internal void Start()
    {
        _receiveLoop = ReceiveLoopAsync(_cts.Token);
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            UdpReceiveResult result;
            try
            {
                result = await _udpClient.ReceiveAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[UDP:{Port}] Receive error: {ex.Message}");
                continue;
            }

            try
            {
                await _udpClient.SendAsync(result.Buffer.AsMemory(), result.RemoteEndPoint, ct);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[UDP:{Port}] Send error: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _udpClient.Close();
        _receiveLoop?.Wait(TimeSpan.FromSeconds(2));
        _cts.Dispose();
    }
}
