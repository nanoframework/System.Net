//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Networking;
using nanoFramework.TestFramework;
using NFUnitTestSocketTests;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

#if HAS_WIFI
using System.Device.Wifi;
#endif

namespace NFUnitTestSslStream
{
    /// <summary>
    /// Exercises the device acting as a TLS server (AuthenticateAsServer / SecureAccept)
    /// and as a TLS client against the Network Test Companion.
    ///
    /// The device loads a static self-signed test certificate (embedded below) to serve.
    /// The companion connects as a TLS client with certificate verification disabled.
    /// For the reverse direction the companion runs a TLS echo server (using the same
    /// static certificate) and the device connects with <see cref="SslVerification.NoVerification"/>.
    ///
    /// Requires the Network Test Companion running on the host PC. Start it with:
    ///   dotnet run --project Tests/NetworkTestCompanion
    /// </summary>
    [TestClass]
    public class SslServerTests
    {
        private static bool _networkInitialized = false;

        [Setup]
        public void Setup()
        {
            // Comment the next line to run these tests on real hardware with network connectivity
            //Assert.SkipTest("Skipping SSL server tests: requires real hardware with companion running");

            CancellationTokenSource cs = new(60000);

#if HAS_WIFI
            bool connected = WifiNetworkHelper.Reconnect(requiresDateTime: true, token: cs.Token);
#else
            bool connected = NetworkHelper.SetupAndConnectNetwork(requiresDateTime: true, token: cs.Token);
#endif

            if (!connected)
            {
                Assert.SkipTest($"Network not available ({NetworkHelper.Status}) - skipping SSL server tests");
            }

            _networkInitialized = true;
            OutputHelper.WriteLine($"Network ready, status: {NetworkHelper.Status}");
        }

        [Cleanup]
        public void Cleanup()
        {
            if (_networkInitialized)
            {
                NetworkHelper.Reset();
                _networkInitialized = false;
            }
        }

        [TestMethod]
        public void DeviceAsServer_CompanionConnects_HandshakeSucceeds()
        {
            const int port = 7010;

            X509Certificate2 serverCert = new X509Certificate2(ServerCertPem, ServerKeyPem, null);

            using (CompanionClient companion = new CompanionClient())
            {
                Assert.IsTrue(companion.Ping(), "Companion not reachable");

                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, port));
                listener.Listen(1);

                try
                {
                    string mcuIp = GetLocalIp();

                    Assert.IsTrue(companion.TlsConnectTo(mcuIp, port), "TlsConnectTo command failed");

                    Assert.IsTrue(listener.Poll(10 * 1000 * 1000, SelectMode.SelectRead),
                        "No connection received from companion within timeout");

                    Socket accepted = listener.Accept();

                    try
                    {
                        using (SslStream sslStream = new SslStream(accepted))
                        {
                            AuthenticateAsServerLogged(sslStream, serverCert);
                            OutputHelper.WriteLine("TLS server handshake succeeded");
                        }
                    }
                    finally
                    {
                        accepted.Close();
                    }
                }
                finally
                {
                    listener.Close();
                }
            }
        }

        [TestMethod]
        public void DeviceAsServer_TlsEchoRoundTrip()
        {
            const int port = 7011;
            byte[] testData = Encoding.UTF8.GetBytes("Hello from companion to device TLS server!");

            X509Certificate2 serverCert = new X509Certificate2(ServerCertPem, ServerKeyPem, null);

            using (CompanionClient companion = new CompanionClient())
            {
                Assert.IsTrue(companion.Ping(), "Companion not reachable");

                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, port));
                listener.Listen(1);

                try
                {
                    string mcuIp = GetLocalIp();

                    // Companion connects, handshakes, sends data and reads the echo (all in
                    // the background); returns Ok as soon as the TCP connection is up.
                    Assert.IsTrue(companion.TlsConnectEcho(mcuIp, port, testData),
                        "TlsConnectEcho command failed");

                    Assert.IsTrue(listener.Poll(10 * 1000 * 1000, SelectMode.SelectRead),
                        "No connection received from companion within timeout");

                    Socket accepted = listener.Accept();

                    try
                    {
                        using (SslStream sslStream = new SslStream(accepted))
                        {
                            AuthenticateAsServerLogged(sslStream, serverCert);
                            OutputHelper.WriteLine("TLS server handshake succeeded");

                            byte[] buffer = new byte[1024];
                            int bytesRead = sslStream.Read(buffer, 0, buffer.Length);

                            Assert.IsTrue(bytesRead > 0, "Should have received data from companion");
                            Assert.AreEqual(testData.Length, bytesRead, "Should receive exact bytes sent by companion");

                            for (int i = 0; i < bytesRead; i++)
                            {
                                Assert.AreEqual(testData[i], buffer[i], $"Data mismatch at byte {i}");
                            }

                            // Echo it back to the companion
                            sslStream.Write(buffer, 0, bytesRead);

                            OutputHelper.WriteLine($"Echoed {bytesRead} bytes back to companion");
                        }
                    }
                    finally
                    {
                        accepted.Close();
                    }
                }
                finally
                {
                    listener.Close();
                }
            }
        }

        [TestMethod]
        public void DeviceAsClient_TlsEchoRoundTrip()
        {
            const int port = 7012;

            using (CompanionClient companion = new CompanionClient())
            {
                Assert.IsTrue(companion.Ping(), "Companion not reachable");
                Assert.IsTrue(companion.StartTlsEcho(port), "StartTlsEcho command failed");

                try
                {
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(new IPEndPoint(
                        IPAddress.Parse(TestConfiguration.CompanionIP),
                        port));

                    using (SslStream sslStream = new SslStream(socket))
                    {
                        sslStream.SslVerification = SslVerification.NoVerification;
                        sslStream.AuthenticateAsClient("nanoFramework Test Server", SslProtocols.Tls12);

                        OutputHelper.WriteLine("TLS client handshake to companion succeeded");

                        byte[] sent = Encoding.UTF8.GetBytes("Hello TLS echo from device!");
                        sslStream.Write(sent, 0, sent.Length);

                        byte[] buffer = new byte[1024];
                        int bytesRead = sslStream.Read(buffer, 0, buffer.Length);

                        Assert.AreEqual(sent.Length, bytesRead, "Echo response length should match sent data");

                        for (int i = 0; i < sent.Length; i++)
                        {
                            Assert.AreEqual(sent[i], buffer[i], $"Echo mismatch at byte {i}");
                        }

                        OutputHelper.WriteLine($"TLS echo round-trip succeeded: {bytesRead} bytes");
                    }
                }
                finally
                {
                    companion.Stop(port);
                }
            }
        }

        [TestMethod]
        public void DeviceAsClient_MultiChunk_TlsEchoRoundTrip()
        {
            const int port = 7013;
            // 8 KB total, exchanged as a chunked ping-pong: write a chunk, read its echo
            // back, then send the next chunk. This spans many SecureWrite/SecureRead calls
            // (and multiple TLS records) while never keeping more than one chunk in flight.
            // A single Write(8192) followed by a read-all would risk a half-duplex echo
            // stall on constrained devices, since neither side drains the other's echo
            // until the whole payload is buffered.
            const int totalSize = 8 * 1024;
            const int chunkSize = 1024;

            using (CompanionClient companion = new CompanionClient())
            {
                Assert.IsTrue(companion.Ping(), "Companion not reachable");
                Assert.IsTrue(companion.StartTlsEcho(port), "StartTlsEcho command failed");

                try
                {
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(new IPEndPoint(
                        IPAddress.Parse(TestConfiguration.CompanionIP),
                        port));

                    using (SslStream sslStream = new SslStream(socket))
                    {
                        sslStream.SslVerification = SslVerification.NoVerification;
                        sslStream.AuthenticateAsClient("nanoFramework Test Server", SslProtocols.Tls12);

                        byte[] chunk = new byte[chunkSize];
                        byte[] received = new byte[chunkSize];
                        int grandTotal = 0;

                        for (int sentSoFar = 0; sentSoFar < totalSize; sentSoFar += chunkSize)
                        {
                            // Fill the chunk with a position-dependent pattern so a
                            // mis-ordered or corrupted echo is detected.
                            for (int i = 0; i < chunkSize; i++)
                            {
                                chunk[i] = (byte)((sentSoFar + i) & 0xFF);
                            }

                            sslStream.Write(chunk, 0, chunkSize);

                            // Read this chunk's echo fully before sending the next one.
                            int chunkRead = 0;
                            while (chunkRead < chunkSize)
                            {
                                int bytesRead = sslStream.Read(received, chunkRead, chunkSize - chunkRead);
                                if (bytesRead == 0)
                                {
                                    break;
                                }

                                chunkRead += bytesRead;
                            }

                            Assert.AreEqual(chunkSize, chunkRead, $"Short echo for chunk at offset {sentSoFar}");

                            for (int i = 0; i < chunkSize; i++)
                            {
                                Assert.AreEqual(chunk[i], received[i],
                                    $"Echo mismatch at byte {sentSoFar + i}");
                            }

                            grandTotal += chunkRead;

                            OutputHelper.WriteLine($"Chunk {sentSoFar / chunkSize + 1}/{totalSize / chunkSize}: echoed {chunkRead} bytes OK (total {grandTotal}/{totalSize})");
                        }

                        Assert.AreEqual(totalSize, grandTotal, "Should echo the whole payload back");

                        OutputHelper.WriteLine($"Multi-chunk TLS echo round-trip succeeded: {grandTotal} bytes");
                    }
                }
                finally
                {
                    companion.Stop(port);
                }
            }
        }

        [TestMethod]
        public void DeviceAsClient_WriteAfterPeerClose_ShouldThrow()
        {
            // Regression test: SslStream.Write used to discard the error returned by
            // native ssl_write_internal, so writing to a dead connection returned
            // normally and the data was silently lost. A write to a peer that has
            // gone away must now fail rather than silently succeed.
            const int port = 7015;

            using (CompanionClient companion = new CompanionClient())
            {
                Assert.IsTrue(companion.Ping(), "Companion not reachable");
                Assert.IsTrue(companion.StartTlsEcho(port), "StartTlsEcho command failed");

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(new IPEndPoint(
                    IPAddress.Parse(TestConfiguration.CompanionIP),
                    port));

                using (SslStream sslStream = new SslStream(socket))
                {
                    sslStream.SslVerification = SslVerification.NoVerification;
                    sslStream.AuthenticateAsClient("nanoFramework Test Server", SslProtocols.Tls12);

                    // Bound how long a write can block. On a dead peer the native write path waits
                    // for the socket to become writable; without a finite timeout it blocks
                    // indefinitely (hanging the test) instead of failing. Must be set now, while the
                    // socket is still alive - setsockopt on an already-reset socket throws.
                    socket.SendTimeout = 3000;

                    // 1. Prove the connection works with one echo round-trip
                    byte[] probe = Encoding.UTF8.GetBytes("alive");
                    sslStream.Write(probe, 0, probe.Length);

                    byte[] probeBack = new byte[probe.Length];
                    int probeRead = 0;
                    while (probeRead < probe.Length)
                    {
                        int n = sslStream.Read(
                            probeBack,
                            probeRead,
                            probe.Length - probeRead);

                        if (n == 0)
                        {
                            break;
                        }

                        probeRead += n;
                    }

                    Assert.AreEqual(probe.Length, probeRead, "Initial echo round-trip failed");
                    OutputHelper.WriteLine("Initial echo round-trip OK - connection is live");

                    // 2. Kill the companion echo server, tearing down the peer TLS session
                    Assert.IsTrue(companion.Stop(port), "Stop command failed");
                    OutputHelper.WriteLine("Companion echo server stopped - peer connection is dead");

                    // Let the TCP RST / FIN propagate
                    Thread.Sleep(2000);

                    // 3. Write to the dead stream - this SHOULD throw once the connection is known bad.
                    // A single write is not a reliable trigger: on a freshly-closed TCP peer the first
                    // send() after close usually succeeds locally (bytes are just queued in the local
                    // send buffer) and the failure only surfaces on a later write, once the RST has
                    // been received or the send buffer can't drain. So write repeatedly - each call
                    // bounded by SendTimeout - until one fails. The underlying failure can surface as
                    // either a native SocketException or the managed IOException guard, so it's
                    // normalized to IOException before being re-thrown for the assertion.
                    byte[] payload = Encoding.UTF8.GetBytes("this should fail");
                    const int maxAttempts = 5;

                    Assert.ThrowsException(
                        typeof(IOException),
                        () =>
                        {
                            for (int attempt = 1; attempt <= maxAttempts; attempt++)
                            {
                                try
                                {
                                    sslStream.Write(payload, 0, payload.Length);
                                    OutputHelper.WriteLine($"Attempt {attempt}/{maxAttempts}: Write({payload.Length} bytes) returned without error on a dead connection");
                                    Thread.Sleep(500);
                                }
                                catch (Exception ex)
                                {
                                    OutputHelper.WriteLine($"Attempt {attempt}/{maxAttempts}: Write failed as expected: {ex.GetType().Name}: {ex.Message}");
                                    throw new IOException("Write failed on dead connection", ex);
                                }
                            }
                        },
                        $"SslStream.Write should have thrown on a dead connection within {maxAttempts} attempts but returned silently every time - data was lost");
                }

                // Explicit cleanup
                socket.Close();
                Thread.Sleep(500);
            }
        }

        [TestMethod]
        public void DeviceAsServer_MultipleSequentialConnections()
        {
            const int port = 7014;
            const int connectionCount = 3;

            using (CompanionClient companion = new CompanionClient())
            {
                Assert.IsTrue(companion.Ping(), "Companion not reachable");

                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, port));
                listener.Listen(1);

                try
                {
                    string mcuIp = GetLocalIp();

                    // Load the certificate once and reuse it across connections. Allocating it
                    // per iteration (each decoding the private key natively) wastes memory and
                    // risks heap fragmentation on the device.
                    X509Certificate2 serverCert = new X509Certificate2(ServerCertPem, ServerKeyPem, null);

                    for (int i = 0; i < connectionCount; i++)
                    {
                        OutputHelper.WriteLine($"Connection {i + 1}/{connectionCount}");

                        Assert.IsTrue(companion.TlsConnectTo(mcuIp, port),
                            $"TlsConnectTo command failed on iteration {i + 1}");

                        Assert.IsTrue(listener.Poll(10 * 1000 * 1000, SelectMode.SelectRead),
                            $"No connection on iteration {i + 1}");

                        Socket accepted = listener.Accept();

                        try
                        {
                            using (SslStream sslStream = new SslStream(accepted))
                            {
                                AuthenticateAsServerLogged(sslStream, serverCert);
                                OutputHelper.WriteLine($"Handshake {i + 1} succeeded");
                            }
                        }
                        finally
                        {
                            accepted.Close();
                        }

                        // Brief pause between connections to let the SSL context slot be freed
                        Thread.Sleep(500);
                    }

                    OutputHelper.WriteLine($"All {connectionCount} sequential TLS connections succeeded");
                }
                finally
                {
                    listener.Close();
                }
            }
        }

        // Wraps AuthenticateAsServer to surface the native error on failure.
        // Passes clientCertificateRequired: false so the server does NOT ask the client
        // for a certificate - otherwise the default SslVerification.CertificateRequired
        // makes mbedTLS abort the handshake with -0x7480 (NO_CLIENT_CERTIFICATE).
        // On failure, ErrorCode is either a small SslError enum value (context init:
        // 6=key, 7=cert, 8=own-cert-config, 9=setup/OOM) or, during the handshake, the
        // raw negative mbedTLS error code (e.g. -0x7480).
        private static void AuthenticateAsServerLogged(SslStream sslStream, X509Certificate2 serverCert)
        {
            try
            {
                sslStream.AuthenticateAsServer(serverCert, false, SslProtocols.Tls12);
            }
            catch (CryptographicException ex)
            {
                OutputHelper.WriteLine($"AuthenticateAsServer failed: ErrorCode={ex.ErrorCode}");
                throw;
            }
        }

        private static string GetLocalIp()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.IPv4Address == null || ni.IPv4Address == "0.0.0.0")
                {
                    continue;
                }

                if (ni.IPv4Address.StartsWith("127.") || ni.IPv4Address.StartsWith("169.254."))
                {
                    continue;
                }

                return ni.IPv4Address;
            }

            return "0.0.0.0";
        }

        // Static self-signed RSA-2048 test certificate, CN=nanoFramework Test Server.
        // The SAME cert/key is embedded in the companion (TestCertificates.cs).
        // Throw-away test certificate - not a secret. Valid 2026..2036.
        private const string ServerCertPem =
@"-----BEGIN CERTIFICATE-----
MIIDRTCCAi2gAwIBAgIUepRBLWtpFLvLv6rjIIUQxmkVkYkwDQYJKoZIhvcNAQEL
BQAwJDEiMCAGA1UEAwwZbmFub0ZyYW1ld29yayBUZXN0IFNlcnZlcjAeFw0yNjA3
MTYxMjA0MDJaFw0zNjA3MTMxMjA0MDJaMCQxIjAgBgNVBAMMGW5hbm9GcmFtZXdv
cmsgVGVzdCBTZXJ2ZXIwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCu
+yM+X9ZcaawdwfhpJiWa4qlrA/1aV0CoENchMP6XOr4Eq7h/Y8jH+QlKdG2hFe31
wULiwLJq6QwTQ23a7vRFBgTZCZJSs5QY54o2r7O6pO37Y1w+/d0/4blFLNWd0PQq
Mm8TUKdK3J11dv+n/oY9++4vFHR6Bo3xjHFBvm03vcKETeF3UIX+g6J84lfNmdPs
A3UIFqkWXioC7a2+afnRczAHrrS0Py2KcSv+G5E94ZYQHs0VljY8CpOEV2maxh9S
Bjocv4o6HUejKoWvbXqkftuxztjYx77p++jhICpnNZjpNOb27rJhtGw3HPwtn8IY
I3jIZS72insBEQgKSxBhAgMBAAGjbzBtMB0GA1UdDgQWBBR7BiZTDR7gx4gl2fV1
Y9fjrb1mMjAfBgNVHSMEGDAWgBR7BiZTDR7gx4gl2fV1Y9fjrb1mMjAJBgNVHRME
AjAAMAsGA1UdDwQEAwIFoDATBgNVHSUEDDAKBggrBgEFBQcDATANBgkqhkiG9w0B
AQsFAAOCAQEAIeCjTx10sJiacjzajLUJ/dfgSq8VfMUhHQLxG5VF3bPMVbr8LaF+
sM77OKXOBKigDj65urMUwAoLKXF7UcewvVV239Og1p97acetDDiM3Q72duH8MFAF
K2V9qR6Cj8/jvQdSHWiJVOHE0u31PG6Xwa6aIwXA79VKbJaucvj9hXYz0O3HXTRc
RmvHemTge0p0VhuTL+wNv46mftEZhoSsPZa4S08nv5VX3EyWfQM6eX3ghnq6CsE+
MQw94r5CV1kZDA/R9IdXh4aRIVCyN0ZmMvfNNrmIJRdr/eLzQc/6DBeh5Wrg7Sc/
9oUAfyIiPqXG3sW0Txof2L5qbTmallrooQ==
-----END CERTIFICATE-----";

        // PKCS#1 (traditional RSA) encoding - mbedTLS on the device configures the
        // own-cert key from this format; PKCS#8 ("BEGIN PRIVATE KEY") fails SecureServerInit.
        private const string ServerKeyPem =
@"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEArvsjPl/WXGmsHcH4aSYlmuKpawP9WldAqBDXITD+lzq+BKu4
f2PIx/kJSnRtoRXt9cFC4sCyaukME0Nt2u70RQYE2QmSUrOUGOeKNq+zuqTt+2Nc
Pv3dP+G5RSzVndD0KjJvE1CnStyddXb/p/6GPfvuLxR0egaN8YxxQb5tN73ChE3h
d1CF/oOifOJXzZnT7AN1CBapFl4qAu2tvmn50XMwB660tD8tinEr/huRPeGWEB7N
FZY2PAqThFdpmsYfUgY6HL+KOh1HoyqFr216pH7bsc7Y2Me+6fvo4SAqZzWY6TTm
9u6yYbRsNxz8LZ/CGCN4yGUu9op7AREICksQYQIDAQABAoIBAB8L2Bj9EB+dcDhn
bhfZ+NoeVUjzkEQzLvmi40i0VLeoaIaToUyY+8rfWNKpDbqDFZGBFMj+v6lQaCAS
2q75rsWAZ+PKWvfpfOFeU5uYWR9InCD6ZCeZC2SGPEUVy2EQ7gF+qU6YBNa3hgiN
cJbyBgeBZ6Vaz7/G4fB1prKvgtlcunjtXAwdme9nkHR2kuG+pGGtNs/qc71bQeOt
5gphHdls+lHX+D6QD/gB2biR1bSJW+Cegz0zNM0nUUz6cA3K9dUTMeyGyI4c8FNt
u/FuRZlri/I7yUAPngVnmq46rJg1Ih0OvXI+jIEHp7SI0e9MiRLnZhB4x3/76940
qAJ+nnECgYEA5692QqZ0eovLlqlWSrRNP0mG31HxCTrWYvfvfWkfhGPskdsJoOeQ
RXk5Mvfp84miIezv8aXxmQEquxQiP8HRtzQ0TrsvKdlM9XOGwUJULyqtFfSs6vmj
HTD16GGPxWqEy0xAcaQcM31pb8YwuZ5MCNRdhuL4ladWBfMR0z75GssCgYEAwVg8
hPsX+H4LzCfu32MQCMh+sAR+1xPTLSG3ydJFbx6PE9pX408ZJKc9CtkG8Xz29+y1
EZnymB1IUpNZxms/4pybyFaKXUa20SpPgSoIrBaL/wdgM4h4Pvs2FSceYN9qY7Z5
d4DhJEiuez+CAFVqQnrNaLJI7xD094SEv91nQAMCgYA+rq8dOzG6UgYj3e61yXA4
1ijCVMYUzDFil1fZI07en7ZKg+tn+B6FXVXHX2GRfUQ7T4Jfa5kg3zrzYHAftc2K
dnpMbsJE3UDAC6CCuvJRzIcFsKvz6tRhunRdib+/FqGU6y1oUZE7sQuMrR9TqOtD
XEltjAzbWGmitG+3Kot03wKBgQCunriaCgWOQpj5HB/b1aaHqDzzUDwWmCskGc3a
E3TudRUYAx1ZiPjWZ8zz3SsuM4UCSeEHMpkt1VSab8anM/oQ+wyflbmFoPZAVwxT
RdlrQznRbaHvKRQhHdWsqRYAvAdkY0u1KMsucA5V9fe9wWck/7BBHLROZmw4mJEk
kBxObQKBgQCCrcUNFhm3dxCdi+VgMwrhMOqiO5XYAGw4raQ/BbXwpb1PLbj4xtSZ
WVb3utTBP0WPhf58EcHc8ko4B+1xCMR4B9rntQACfngbUN4wETQ1Gz+bNgaHQyJo
gHYA38gnEOJurr2VLZFaqLgwj+7kpTRL2a0ZTDz8pCxlwDpsi4n1YA==
-----END RSA PRIVATE KEY-----";
    }
}
