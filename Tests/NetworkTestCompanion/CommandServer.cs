// Copyright (c) .NET Foundation and Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NetworkTestCompanion;

/// <summary>
/// Control channel (default port 11000). Accepts newline-delimited JSON commands from MCU tests
/// and manages the lifecycle of TCP/UDP echo servers on demand.
///
/// Supported commands:
///   { "cmd": "ping" }
///   { "cmd": "start_tcp_echo", "port": N }
///   { "cmd": "start_udp_echo", "port": N }
///   { "cmd": "stop", "port": N }
///   { "cmd": "stop_all" }
///   { "cmd": "connect_to", "host": "...", "port": N }
///   { "cmd": "start_tls_echo", "port": N }
///   { "cmd": "tls_connect_to", "host": "...", "port": N }
///   { "cmd": "tls_connect_echo", "host": "...", "port": N, "data": "base64..." }
/// </summary>
internal sealed class CommandServer : IDisposable
{
    private readonly TcpListener _listener;
    private readonly IPAddress _bindAddress;
    private readonly CancellationTokenSource _cts = new();
    private readonly Dictionary<int, IDisposable> _activeServers = [];
    private readonly Lock _lock = new();
    private Task? _acceptLoop;

    internal CommandServer(IPAddress bindAddress, int port)
    {
        _bindAddress = bindAddress;
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
                Console.Error.WriteLine($"[CMD] Accept error: {ex.Message}");
                continue;
            }

            _ = HandleClientAsync(client, ct);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        {
            using var reader = new StreamReader(client.GetStream(), Encoding.UTF8, leaveOpen: true);
            using var writer = new StreamWriter(client.GetStream(), Encoding.UTF8, leaveOpen: true) { AutoFlush = true };

            try
            {
                string? line;
                while ((line = await reader.ReadLineAsync(ct)) != null)
                {
                    var response = ProcessCommand(line.Trim());
                    await writer.WriteLineAsync(response);
                }
            }
            catch (OperationCanceledException) { }
            catch (IOException) { }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[CMD] Handler error: {ex.Message}");
            }
        }
    }

    private string ProcessCommand(string json)
    {
        JsonNode? node;
        try
        {
            node = JsonNode.Parse(json);
            if (node == null) return Error("empty command");
        }
        catch
        {
            return Error("invalid JSON");
        }

        var cmd = node["cmd"]?.GetValue<string>();
        return cmd switch
        {
            "ping" => Ok(new { ip = _bindAddress.ToString() }),
            "start_tcp_echo" => StartTcpEcho(node),
            "start_udp_echo" => StartUdpEcho(node),
            "stop" => Stop(node),
            "stop_all" => StopAll(),
            "connect_to" => ConnectTo(node),
            "start_tls_echo" => StartTlsEcho(node),
            "tls_connect_to" => TlsConnectTo(node),
            "tls_connect_echo" => TlsConnectEcho(node),
            _ => Error($"unknown command: {cmd}")
        };
    }

    private string StartTcpEcho(JsonNode node)
    {
        if (!TryGetPort(node, out var port, out var err)) return err!;

        lock (_lock)
        {
            if (_activeServers.ContainsKey(port))
                return Error($"port {port} already in use");

            var server = new TcpEchoServer(_bindAddress, port);
            try
            {
                server.Start();
            }
            catch (Exception ex)
            {
                server.Dispose();
                return Error(ex.Message);
            }

            _activeServers[port] = server;
        }

        Console.WriteLine($"[CMD] TCP echo started on port {port}");
        return Ok();
    }

    private string StartUdpEcho(JsonNode node)
    {
        if (!TryGetPort(node, out var port, out var err)) return err!;

        lock (_lock)
        {
            if (_activeServers.ContainsKey(port))
                return Error($"port {port} already in use");

            var server = new UdpEchoServer(_bindAddress, port);
            try
            {
                server.Start();
            }
            catch (Exception ex)
            {
                server.Dispose();
                return Error(ex.Message);
            }

            _activeServers[port] = server;
        }

        Console.WriteLine($"[CMD] UDP echo started on port {port}");
        return Ok();
    }

    private string Stop(JsonNode node)
    {
        if (!TryGetPort(node, out var port, out var err)) return err!;

        lock (_lock)
        {
            if (!_activeServers.TryGetValue(port, out var server))
                return Error($"no server on port {port}");

            server.Dispose();
            _activeServers.Remove(port);
        }

        Console.WriteLine($"[CMD] Stopped server on port {port}");
        return Ok();
    }

    private string StopAll()
    {
        lock (_lock)
        {
            foreach (var server in _activeServers.Values)
                server.Dispose();
            _activeServers.Clear();
        }

        Console.WriteLine("[CMD] All servers stopped");
        return Ok();
    }

    private string ConnectTo(JsonNode node)
    {
        var host = node["host"]?.GetValue<string>();
        if (string.IsNullOrEmpty(host)) return Error("missing 'host'");
        if (!TryGetPort(node, out var port, out var err)) return err!;

        // Connect synchronously so the connection is in the MCU's listen backlog
        // before we return ok and the MCU calls Accept().
        // The client is kept alive asynchronously so the connection isn't torn down
        // before the MCU has had time to accept - a successful Accept() is sufficient
        // proof of connectivity; no probe exchange is needed.
        TcpClient? connectClient = null;
        try
        {
            connectClient = new TcpClient();
            if (!connectClient.ConnectAsync(host, port).Wait(TimeSpan.FromSeconds(5)))
            {
                connectClient.Dispose();
                return Error($"connect to {host}:{port} timed out");
            }

            Console.WriteLine($"[CMD] connect_to {host}:{port} succeeded");

            // Close after the MCU has had time to call Accept().
            var clientToClose = connectClient;
            _ = Task.Delay(2000).ContinueWith(_ => clientToClose.Dispose());

            return Ok();
        }
        catch (Exception ex)
        {
            connectClient?.Dispose();
            Console.Error.WriteLine($"[CMD] connect_to {host}:{port} failed: {ex.Message}");
            return Error(ex.Message);
        }
    }

    private string StartTlsEcho(JsonNode node)
    {
        if (!TryGetPort(node, out var port, out var err)) return err!;

        lock (_lock)
        {
            // Replace any stale server left registered on this port by a previous
            // (possibly aborted) test run — the companion is long-lived across runs.
            if (_activeServers.TryGetValue(port, out var existing))
            {
                existing.Dispose();
                _activeServers.Remove(port);
                Console.WriteLine($"[CMD] Replaced stale server on port {port}");
            }

            var server = new TlsEchoServer(_bindAddress, port, TestCertificates.ServerCert);
            try
            {
                server.Start();
            }
            catch (Exception ex)
            {
                server.Dispose();
                return Error(ex.Message);
            }

            _activeServers[port] = server;
        }

        Console.WriteLine($"[CMD] TLS echo started on port {port}");
        return Ok();
    }

    private string TlsConnectTo(JsonNode node)
    {
        var host = node["host"]?.GetValue<string>();
        if (string.IsNullOrEmpty(host)) return Error("missing 'host'");
        if (!TryGetPort(node, out var port, out var err)) return err!;

        TcpClient? connectClient = null;
        try
        {
            connectClient = new TcpClient();
            if (!connectClient.ConnectAsync(host, port).Wait(TimeSpan.FromSeconds(5)))
            {
                connectClient.Dispose();
                return Error($"connect to {host}:{port} timed out");
            }

            Console.WriteLine($"[CMD] tls_connect_to {host}:{port} TCP connected, starting TLS handshake in background");

            // TLS handshake + keep-alive runs in background so the device
            // can call Accept() and AuthenticateAsServer() after getting Ok.
            var clientForBg = connectClient;
            _ = Task.Run(async () =>
            {
                try
                {
                    var sslStream = new SslStream(
                        clientForBg.GetStream(),
                        leaveInnerStreamOpen: false,
                        (_, _, _, _) => true);

                    await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                    {
                        TargetHost = host,
                        EnabledSslProtocols = SslProtocols.Tls12
                    });

                    Console.WriteLine($"[CMD] tls_connect_to {host}:{port} TLS handshake succeeded");

                    await Task.Delay(2000);
                    sslStream.Dispose();
                    clientForBg.Dispose();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[CMD] tls_connect_to {host}:{port} TLS failed: {ex.Message}");
                    clientForBg.Dispose();
                }
            });

            return Ok();
        }
        catch (Exception ex)
        {
            connectClient?.Dispose();
            Console.Error.WriteLine($"[CMD] tls_connect_to {host}:{port} failed: {ex.Message}");
            return Error(ex.Message);
        }
    }

    private string TlsConnectEcho(JsonNode node)
    {
        var host = node["host"]?.GetValue<string>();
        if (string.IsNullOrEmpty(host)) return Error("missing 'host'");
        if (!TryGetPort(node, out var port, out var err)) return err!;
        var dataB64 = node["data"]?.GetValue<string>();
        if (string.IsNullOrEmpty(dataB64)) return Error("missing 'data'");

        byte[] dataToSend;
        try { dataToSend = Convert.FromBase64String(dataB64); }
        catch { return Error("'data' is not valid base64"); }

        TcpClient? connectClient = null;
        try
        {
            connectClient = new TcpClient();
            if (!connectClient.ConnectAsync(host, port).Wait(TimeSpan.FromSeconds(5)))
            {
                connectClient.Dispose();
                return Error($"connect to {host}:{port} timed out");
            }

            Console.WriteLine($"[CMD] tls_connect_echo {host}:{port} TCP connected, starting TLS + echo in background");

            // TLS handshake + echo runs in background so the device
            // can call Accept() and AuthenticateAsServer() after getting Ok.
            var clientForBg = connectClient;
            _ = Task.Run(async () =>
            {
                try
                {
                    var sslStream = new SslStream(
                        clientForBg.GetStream(),
                        leaveInnerStreamOpen: false,
                        (_, _, _, _) => true);

                    await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                    {
                        TargetHost = host,
                        EnabledSslProtocols = SslProtocols.Tls12
                    });

                    await sslStream.WriteAsync(dataToSend);
                    await sslStream.FlushAsync();

                    Console.WriteLine($"[CMD] tls_connect_echo {host}:{port}: sent {dataToSend.Length} bytes, waiting for echo");

                    var buf = new byte[4096];
                    int totalRead = 0;
                    using var ms = new MemoryStream();

                    while (totalRead < dataToSend.Length)
                    {
                        int read = await sslStream.ReadAsync(buf);
                        if (read == 0) break;
                        ms.Write(buf, 0, read);
                        totalRead += read;
                    }

                    Console.WriteLine($"[CMD] tls_connect_echo {host}:{port}: received {totalRead} bytes echo");

                    sslStream.Dispose();
                    clientForBg.Dispose();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[CMD] tls_connect_echo {host}:{port} TLS/echo failed: {ex.Message}");
                    clientForBg.Dispose();
                }
            });

            return Ok();
        }
        catch (Exception ex)
        {
            connectClient?.Dispose();
            Console.Error.WriteLine($"[CMD] tls_connect_echo {host}:{port} failed: {ex.Message}");
            return Error(ex.Message);
        }
    }

    private static bool TryGetPort(JsonNode node, out int port, out string? error)
    {
        port = 0;
        error = null;
        var portNode = node["port"];
        if (portNode == null) { error = Error("missing 'port'"); return false; }
        try { port = portNode.GetValue<int>(); return true; }
        catch { error = Error("'port' must be an integer"); return false; }
    }

    private static string Ok(object? extra = null)
    {
        if (extra == null) return "{\"ok\":true}";
        var extraJson = JsonSerializer.Serialize(extra);
        // Merge { "ok": true } with the extra object
        var merged = $"{{\"ok\":true,{extraJson.TrimStart('{').TrimEnd('}')}}}";
        return merged.Replace(",}", "}");
    }

    private static string Error(string message) =>
        JsonSerializer.Serialize(new { ok = false, error = message });

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();
        lock (_lock)
        {
            foreach (var server in _activeServers.Values)
                server.Dispose();
            _activeServers.Clear();
        }
        _acceptLoop?.Wait(TimeSpan.FromSeconds(2));
        _cts.Dispose();
    }
}
