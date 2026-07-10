// Copyright (c) .NET Foundation and Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Sockets;

namespace NetworkTestCompanion;

/// <summary>
/// Listens on a TCP port and echoes all received bytes back to the sender.
/// </summary>
internal sealed class TcpEchoServer : IDisposable
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private Task? _acceptLoop;

    internal int Port { get; }

    internal TcpEchoServer(IPAddress bindAddress, int port)
    {
        Port = port;
        _listener = new TcpListener(bindAddress, port);
    }

    internal void Start()
    {
        _listener.Start();
        _acceptLoop = AcceptLoopAsync(_cts.Token);
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[TCP:{Port}] Accept error: {ex.Message}");
                continue;
            }

            _ = HandleClientAsync(client, ct);
        }
    }

    private static async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        {
            var stream = client.GetStream();
            var buf = new byte[4096];
            try
            {
                int read;

                while ((read = await stream.ReadAsync(buf, ct)) > 0)
                {
                    await stream.WriteAsync(buf.AsMemory(0, read), ct);
                }
            }
            catch (OperationCanceledException) { }
            catch (IOException) { }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
        _acceptLoop?.Wait(TimeSpan.FromSeconds(2));
        _cts.Dispose();
    }
}
