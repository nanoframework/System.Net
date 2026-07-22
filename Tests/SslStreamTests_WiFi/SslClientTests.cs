//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Networking;
using nanoFramework.TestFramework;
using System;
using System.Net;
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
    /// Exercises the device acting as a TLS client.
    /// </summary>
    [TestClass]
    public class SslClientTests
    {
        private const string TestHost = "www.howsmyssl.com";
        private const int HttpsPort = 443;

        private static bool _networkInitialized = false;

        [Setup]
        public void Setup()
        {
            // Comment the next line to run these tests on real hardware with network connectivity
            Assert.SkipTest("Skipping SSL tests: requires real hardware with network connectivity");

            CancellationTokenSource cs = new(60000);

#if HAS_WIFI
            bool connected = WifiNetworkHelper.Reconnect(requiresDateTime: true, token: cs.Token);
#else
            bool connected = NetworkHelper.SetupAndConnectNetwork(requiresDateTime: true, token: cs.Token);
#endif

            if (!connected)
            {
                Assert.SkipTest($"Network not available ({NetworkHelper.Status}) - skipping SSL tests");
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
        public void ConnectToSecureServer_WithNoVerification_Succeeds_Tls12()
        {
            ConnectToSecureServer_WithNoVerification_Succeeds(SslProtocols.Tls12);
        }

        [TestMethod]
        public void ConnectToSecureServer_WithNoVerification_Succeeds_Tls13()
        {
            ConnectToSecureServer_WithNoVerification_Succeeds(SslProtocols.Tls13);
        }

        private void ConnectToSecureServer_WithNoVerification_Succeeds(SslProtocols protocol)
        {
            using (Socket socket = ConnectSocket(TestHost, HttpsPort))
            using (SslStream sslStream = new SslStream(socket))
            {
                try
                {
                    // Skip certificate verification - focuses on handshake mechanics
                    sslStream.SslVerification = SslVerification.NoVerification;
                    sslStream.AuthenticateAsClient(TestHost, protocol);

                    OutputHelper.WriteLine($"TLS handshake succeeded (no verification, {protocol})");

                    string response = SendHttpGetAndReadResponse(sslStream);

                    Assert.IsTrue(
                        response.IndexOf("tls_version") > -1,
                        "Response should contain tls_version confirming TLS negotiation");

                    OutputHelper.WriteLine("Successfully received TLS check response");
                }
                catch (InvalidOperationException ex)
                {
                    // Handshake context errors: HandshakeBadContext, HandshakeSetHostname
                    OutputHelper.WriteLine($"InvalidOperationException: {ex.Message}");
                    throw;
                }
                catch (CryptographicException ex)
                {
                    // HandshakeCertVerifyFailed: ErrorCode = MBEDTLS_X509_BADCERT_* bitmask
                    // HandshakeFailed: ErrorCode = raw negative mbedTLS error code
                    OutputHelper.WriteLine($"CryptographicException: {ex.Message}");
                    OutputHelper.WriteLine($"  ErrorCode: 0x{ex.ErrorCode:X8}");
                    throw;
                }
            }
        }

        [TestMethod]
        public void ConnectToSecureServer_WithCertVerification_Succeeds_Tls12()
        {
            ConnectToSecureServer_WithCertVerification_Succeeds(SslProtocols.Tls12);
        }

        [TestMethod]
        public void ConnectToSecureServer_WithCertVerification_Succeeds_Tls13()
        {
            ConnectToSecureServer_WithCertVerification_Succeeds(SslProtocols.Tls13);
        }

        private void ConnectToSecureServer_WithCertVerification_Succeeds(SslProtocols protocol)
        {
            X509Certificate caCert = new X509Certificate(ISRGRootX1);

            using (Socket socket = ConnectSocket(TestHost, HttpsPort))
            using (SslStream sslStream = new SslStream(socket))
            {
                try
                {
                    // Provide the CA root certificate for the server
                    sslStream.AuthenticateAsClient(TestHost, null, caCert, protocol);

                    OutputHelper.WriteLine($"TLS handshake succeeded (with certificate verification, {protocol})");

                    string response = SendHttpGetAndReadResponse(sslStream);

                    Assert.IsTrue(
                        response.IndexOf("tls_version") > -1,
                        "Response should contain tls_version confirming TLS negotiation");

                    OutputHelper.WriteLine("Successfully received TLS check response with cert verification");
                }
                catch (InvalidOperationException ex)
                {
                    OutputHelper.WriteLine($"InvalidOperationException: {ex.Message}");
                    throw;
                }
                catch (CryptographicException ex)
                {
                    OutputHelper.WriteLine($"CryptographicException: {ex.Message}");
                    OutputHelper.WriteLine($"  ErrorCode: 0x{ex.ErrorCode:X8}");
                    throw;
                }
            }
        }

        [TestMethod]
        public void ConnectWithWrongCaCert_ThrowsCryptographicException_Tls12()
        {
            ConnectWithWrongCaCert_ThrowsCryptographicException(SslProtocols.Tls12);
        }

        [TestMethod]
        public void ConnectWithWrongCaCert_ThrowsCryptographicException_Tls13()
        {
            ConnectWithWrongCaCert_ThrowsCryptographicException(SslProtocols.Tls13);
        }

        private void ConnectWithWrongCaCert_ThrowsCryptographicException(SslProtocols protocol)
        {
            // Use a self-signed cert that is NOT the CA for howsmyssl.com
            X509Certificate wrongCaCert = new X509Certificate(InvalidCACert);

            using (Socket socket = ConnectSocket(TestHost, HttpsPort))
            using (SslStream sslStream = new SslStream(socket))
            {
                bool exceptionCaught = false;

                try
                {
                    // Provide a wrong CA certificate - the server cert chain won't validate
                    sslStream.AuthenticateAsClient(TestHost, null, wrongCaCert, protocol);
                }
                catch (CryptographicException ex)
                {
                    exceptionCaught = true;

                    OutputHelper.WriteLine($"Expected CryptographicException caught");
                    OutputHelper.WriteLine($"  Message: {ex.Message}");
                    OutputHelper.WriteLine($"  ErrorCode: 0x{ex.ErrorCode:X8}");

                    // ErrorCode should contain MBEDTLS_X509_BADCERT_NOT_TRUSTED (0x08) or similar
                    Assert.IsTrue(
                        ex.ErrorCode != 0,
                        "ErrorCode should be non-zero (MBEDTLS_X509_BADCERT_* bitmask)");
                }

                Assert.IsTrue(exceptionCaught, "Expected an exception when using wrong CA certificate");
            }
        }

        [TestMethod]
        public void ConnectWithBadHostname_ThrowsCryptographicException_Tls12()
        {
            ConnectWithBadHostname_ThrowsCryptographicException(SslProtocols.Tls12);
        }

        [TestMethod]
        public void ConnectWithBadHostname_ThrowsCryptographicException_Tls13()
        {
            ConnectWithBadHostname_ThrowsCryptographicException(SslProtocols.Tls13);
        }

        private void ConnectWithBadHostname_ThrowsCryptographicException(SslProtocols protocol)
        {
            X509Certificate caCert = new X509Certificate(ISRGRootX1);

            using (Socket socket = ConnectSocket(TestHost, HttpsPort))
            using (SslStream sslStream = new SslStream(socket))
            {
                bool exceptionCaught = false;

                try
                {
                    // Authenticate with a mismatched hostname - server cert won't match,
                    // triggering HandshakeCertVerifyFailed with MBEDTLS_X509_BADCERT_CN_MISMATCH
                    sslStream.AuthenticateAsClient("wrong.host.name", null, caCert, protocol);
                }
                catch (CryptographicException ex)
                {
                    exceptionCaught = true;

                    OutputHelper.WriteLine($"Expected CryptographicException caught");
                    OutputHelper.WriteLine($"  Message: {ex.Message}");
                    OutputHelper.WriteLine($"  ErrorCode: 0x{ex.ErrorCode:X8}");

                    Assert.IsTrue(
                        ex.ErrorCode != 0,
                        "ErrorCode should be non-zero (MBEDTLS_X509_BADCERT_* bitmask)");
                }

                Assert.IsTrue(exceptionCaught, "Expected an exception from hostname mismatch authentication");
            }
        }

        [TestMethod]
        public void ConnectWithoutAuth_ThrowsCryptographicException_Tls12()
        {
            ConnectWithoutAuth_ThrowsCryptographicException(SslProtocols.Tls12);
        }

        [TestMethod]
        public void ConnectWithoutAuth_ThrowsCryptographicException_Tls13()
        {
            ConnectWithoutAuth_ThrowsCryptographicException(SslProtocols.Tls13);
        }

        private void ConnectWithoutAuth_ThrowsCryptographicException(SslProtocols protocol)
        {
            using (Socket socket = ConnectSocket(TestHost, HttpsPort))
            using (SslStream sslStream = new SslStream(socket))
            {
                bool exceptionCaught = false;

                try
                {
                    // Certificate verification required (default) but no CA certificate provided
                    // and no device certificate store - should fail with HandshakeCertVerifyFailed
                    sslStream.AuthenticateAsClient(TestHost, protocol);
                }
                catch (CryptographicException ex)
                {
                    exceptionCaught = true;

                    OutputHelper.WriteLine($"Expected CryptographicException caught");
                    OutputHelper.WriteLine($"  Message: {ex.Message}");
                    OutputHelper.WriteLine($"  ErrorCode: 0x{ex.ErrorCode:X8}");

                    Assert.IsTrue(
                        ex.ErrorCode != 0,
                        "ErrorCode should be non-zero (certificate verification failure flags)");
                }

                Assert.IsTrue(exceptionCaught, "Expected an exception when connecting without authentication");
            }
        }

        /// <summary>
        /// Creates a TCP socket and connects to the specified host and port.
        /// </summary>
        private static Socket ConnectSocket(string host, int port)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            IPEndPoint ep = new IPEndPoint(hostEntry.AddressList[0], port);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ep);

            return socket;
        }

        /// <summary>
        /// Sends an HTTP GET request and reads the response.
        /// </summary>
        private static string SendHttpGetAndReadResponse(SslStream sslStream)
        {
            byte[] request = Encoding.UTF8.GetBytes(
                $"GET /a/check HTTP/1.1\r\nHost: {TestHost}\r\nConnection: close\r\n\r\n");

            sslStream.Write(request, 0, request.Length);

            // Use blocking Read - the native ReadWriteHelper handles non-blocking
            // sockets with WaitEvents, so Read() will block until data arrives
            // or the ReceiveTimeout expires. No need to poll DataAvailable.
            byte[] buffer = new byte[1024];
            StringBuilder sb = new StringBuilder();
            int bytesRead;

            do
            {
                bytesRead = sslStream.Read(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                }
            }
            while (bytesRead > 0);

            string response = sb.ToString();
            OutputHelper.WriteLine($"Response length: {response.Length} bytes");

            return response;
        }

        // ISRG Root X1 - root CA for Let's Encrypt (used by www.howsmyssl.com)
        // from https://letsencrypt.org/certificates/
        private const string ISRGRootX1 =
@"-----BEGIN CERTIFICATE-----
MIIFazCCA1OgAwIBAgIRAIIQz7DSQONZRGPgu2OCiwAwDQYJKoZIhvcNAQELBQAw
TzELMAkGA1UEBhMCVVMxKTAnBgNVBAoTIEludGVybmV0IFNlY3VyaXR5IFJlc2Vh
cmNoIEdyb3VwMRUwEwYDVQQDEwxJU1JHIFJvb3QgWDEwHhcNMTUwNjA0MTEwNDM4
WhcNMzUwNjA0MTEwNDM4WjBPMQswCQYDVQQGEwJVUzEpMCcGA1UEChMgSW50ZXJu
ZXQgU2VjdXJpdHkgUmVzZWFyY2ggR3JvdXAxFTATBgNVBAMTDElTUkcgUm9vdCBY
MTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAK3oJHP0FDfzm54rVygc
h77ct984kIxuPOZXoHj3dcKi/vVqbvYATyjb3miGbESTtrFj/RQSa78f0uoxmyF+
0TM8ukj13Xnfs7j/EvEhmkvBioZxaUpmZmyPfjxwv60pIgbz5MDmgK7iS4+3mX6U
A5/TR5d8mUgjU+g4rk8Kb4Mu0UlXjIB0ttov0DiNewNwIRt18jA8+o+u3dpjq+sW
T8KOEUt+zwvo/7V3LvSye0rgTBIlDHCNAymg4VMk7BPZ7hm/ELNKjD+Jo2FR3qyH
B5T0Y3HsLuJvW5iB4YlcNHlsdu87kGJ55tukmi8mxdAQ4Q7e2RCOFvu396j3x+UC
B5iPNgiV5+I3lg02dZ77DnKxHZu8A/lJBdiB3QW0KtZB6awBdpUKD9jf1b0SHzUv
KBds0pjBqAlkd25HN7rOrFleaJ1/ctaJxQZBKT5ZPt0m9STJEadao0xAH0ahmbWn
OlFuhjuefXKnEgV4We0+UXgVCwOPjdAvBbI+e0ocS3MFEvzG6uBQE3xDk3SzynTn
jh8BCNAw1FtxNrQHusEwMFxIt4I7mKZ9YIqioymCzLq9gwQbooMDQaHWBfEbwrbw
qHyGO0aoSCqI3Haadr8faqU9GY/rOPNk3sgrDQoo//fb4hVC1CLQJ13hef4Y53CI
rU7m2Ys6xt0nUW7/vGT1M0NPAgMBAAGjQjBAMA4GA1UdDwEB/wQEAwIBBjAPBgNV
HRMBAf8EBTADAQH/MB0GA1UdDgQWBBR5tFnme7bl5AFzgAiIyBpY9umbbjANBgkq
hkiG9w0BAQsFAAOCAgEAVR9YqbyyqFDQDLHYGmkgJykIrGF1XIpu+ILlaS/V9lZL
ubhzEFnTIZd+50xx+7LSYK05qAvqFyFWhfFQDlnrzuBZ6brJFe+GnY+EgPbk6ZGQ
3BebYhtF8GaV0nxvwuo77x/Py9auJ/GpsMiu/X1+mvoiBOv/2X/qkSsisRcOj/KK
NFtY2PwByVS5uCbMiogziUwthDyC3+6WVwW6LLv3xLfHTjuCvjHIInNzktHCgKQ5
ORAzI4JMPJ+GslWYHb4phowim57iaztXOoJwTdwJx4nLCgdNbOhdjsnvzqvHu7Ur
TkXWStAmzOVyyghqpZXjFaH3pO3JLF+l+/+sKAIuvtd7u+Nxe5AW0wdeRlN8NwdC
jNPElpzVmbUq4JUagEiuTDkHzsxHpFKVK7q4+63SM1N95R1NbdWhscdCb+ZAJzVc
oyi3B43njTOQ5yOf+1CceWxG1bQVs5ZufpsMljq4Ui0/1lvh+wjChP4kqKOJ2qxq
4RgqsahDYVvTH9w7jXbyLeiNdd8XM2w9U/t7y0Ff/9yi0GE44Za4rF2LN9d11TPA
mRGunUHBcnWEvgJBQl9nJEiU0Zsnvgc/ubhPgXRR4Xq37Z0j4r7g1SgEEzwxA57d
emyPxgcYxn/eR44/KJ4EBs+lVDR3veyJm+kXQ99b21/+jh5Xos1AnX5iItreGCc=
-----END CERTIFICATE-----";

        // Frank4DD Web CA — a valid certificate that is NOT the CA for howsmyssl.com.
        // Using it as the CA cert should trigger MBEDTLS_X509_BADCERT_NOT_TRUSTED.
        private const string InvalidCACert =
@"-----BEGIN CERTIFICATE-----
MIIC2jCCAkMCAg38MA0GCSqGSIb3DQEBBQUAMIGbMQswCQYDVQQGEwJKUDEOMAwG
A1UECBMFVG9reW8xEDAOBgNVBAcTB0NodW8ta3UxETAPBgNVBAoTCEZyYW5rNERE
MRgwFgYDVQQLEw9XZWJDZXJ0IFN1cHBvcnQxGDAWBgNVBAMTD0ZyYW5rNEREIFdl
YiBDQTEjMCEGCSqGSIb3DQEJARYUc3VwcG9ydEBmcmFuazRkZC5jb20wHhcNMTIw
ODIyMDUyNzQxWhcNMTcwODIxMDUyNzQxWjBKMQswCQYDVQQGEwJKUDEOMAwGA1UE
CAwFVG9reW8xETAPBgNVBAoMCEZyYW5rNEREMRgwFgYDVQQDDA93d3cuZXhhbXBs
ZS5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC0z9FeMynsC8+u
dvX+LciZxnh5uRj4C9S6tNeeAlIGCfQYk0zUcNFCoCkTknNQd/YEiawDLNbxBqut
bMDZ1aarys1a0lYmUeVLCIqvzBkPJTSQsCopQQ9V8WuT252zzNzs68dVGNdCJd5J
NRQykpwexmnjPPv0mvj7i8XgG379TyW6P+WWV5okeUkXJ9eJS2ouDYdR2SM9BoVW
+FgxDu6BmXhozW5EfsnajFp7HL8kQClI0QOc79yuKl3492rH6bzFsFn2lfwWy9ic
7cP8EpCTeFp1tFaD+vxBhPZkeTQ1HKx6hQ5zeHIB5ySJJZ7af2W8r4eTGYzbdRW2
4DDHCPhZAgMBAAEwDQYJKoZIhvcNAQEFBQADgYEAQMv+BFvGdMVzkQaQ3/+2noVz
/uAKbzpEL8xTcxYyP3lkOeh4FoxiSWqy5pGFALdPONoDuYFpLhjJSZaEwuvjI/Tr
rGhLV1pRG9frwDFshqD2Vaj4ENBCBh6UpeBop5+285zQ4SI7q4U9oSebUDJiuOx6
+tZ9KynmrbJpTSi0+BM=
-----END CERTIFICATE-----";
    }
}
