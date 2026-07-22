// Copyright (c) .NET Foundation and Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace NetworkTestCompanion;

/// <summary>
/// Listens on a TCP port, wraps each accepted connection in an SslStream,
/// and echoes all received bytes back to the sender. Used by the device's
/// "device as TLS client" tests, where the device connects with certificate
/// verification disabled.
/// </summary>
internal sealed class TlsEchoServer : IDisposable
{
    private readonly TcpListener _listener;
    private readonly X509Certificate2 _serverCert;
    private readonly CancellationTokenSource _cts = new();
    private Task? _acceptLoop;

    internal int Port { get; }

    internal TlsEchoServer(IPAddress bindAddress, int port, X509Certificate2 serverCert)
    {
        Port = port;
        _serverCert = serverCert;
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
                Console.Error.WriteLine($"[TLS:{Port}] Accept error: {ex.Message}");
                continue;
            }

            _ = HandleClientAsync(client, ct);
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        {
            SslStream? sslStream = null;
            try
            {
                sslStream = new SslStream(client.GetStream(), leaveInnerStreamOpen: false);

                await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                {
                    ServerCertificate = _serverCert,
                    ClientCertificateRequired = false,
                    EnabledSslProtocols = SslProtocols.Tls12,
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                }, ct);

                Console.WriteLine($"[TLS:{Port}] Client connected, TLS {sslStream.SslProtocol}");

                var buf = new byte[4096];
                int read;

                while ((read = await sslStream.ReadAsync(buf, ct)) > 0)
                {
                    await sslStream.WriteAsync(buf.AsMemory(0, read), ct);
                }
            }
            catch (OperationCanceledException) { }
            catch (IOException) { }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[TLS:{Port}] TLS error: {ex.Message}");
            }
            finally
            {
                if (sslStream != null)
                {
                    await sslStream.DisposeAsync();
                }
            }
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
