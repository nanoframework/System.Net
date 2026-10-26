//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NFUnitTestSocketTests
{
    /// <summary>
    /// Thin client for the Network Test Companion control channel.
    /// Sends newline-terminated JSON commands and reads the JSON response.
    /// Must use synchronous Socket calls (nanoFramework has no Task/async).
    /// </summary>
    internal sealed class CompanionClient : IDisposable
    {
        private readonly Socket _socket;
        private readonly byte[] _recvBuf = new byte[256];

        internal CompanionClient()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _socket.Connect(new IPEndPoint(
                    IPAddress.Parse(TestConfiguration.CompanionIP),
                    TestConfiguration.CompanionControlPort));
            }
            catch (SocketException ex)
            {
                _socket.Close();

                // a bare SocketException from Connect doesn't tell the developer what's missing
                throw new Exception(NotReachableMessage($"connect failed with SocketException {ex.ErrorCode}"));
            }

            // Brief settle time so the companion is ready to read
            Thread.Sleep(50);
        }

        /// <summary>
        /// Connects to the companion and pings it, throwing an exception with setup instructions when it isn't
        /// reachable. Call from [Setup] of test classes that need the companion so a missing companion is reported
        /// once, up front.
        /// </summary>
        internal static void EnsureReachable()
        {
            using (var companion = new CompanionClient())
            {
                if (!companion.Ping())
                {
                    throw new Exception(NotReachableMessage("connected but got no valid reply to ping"));
                }
            }
        }

        private static string NotReachableMessage(string reason) =>
            $"Network Test Companion not reachable at {TestConfiguration.CompanionIP}:{TestConfiguration.CompanionControlPort} ({reason}). "
            + "Start it with 'dotnet run --project Tests/NetworkTestCompanion' and make sure CompanionIP in .runsettings "
            + "matches the LAN IP it prints, then rebuild the test project.";

        /// <summary>Returns true when the companion responds with {"ok":true}.</summary>
        internal bool Ping()
        {
            var response = SendCommand("{\"cmd\":\"ping\"}");
            return response.IndexOf("\"ok\":true") >= 0;
        }

        /// <summary>Asks the companion to start a TCP echo server on the given port.</summary>
        internal bool StartTcpEcho(int port)
        {
            var response = SendCommand("{\"cmd\":\"start_tcp_echo\",\"port\":" + port + "}");
            return response.IndexOf("\"ok\":true") >= 0;
        }

        /// <summary>Asks the companion to start a UDP echo server on the given port.</summary>
        internal bool StartUdpEcho(int port)
        {
            var response = SendCommand("{\"cmd\":\"start_udp_echo\",\"port\":" + port + "}");
            return response.IndexOf("\"ok\":true") >= 0;
        }

        /// <summary>Stops the echo server the companion is running on the given port.</summary>
        internal bool Stop(int port)
        {
            var response = SendCommand("{\"cmd\":\"stop\",\"port\":" + port + "}");
            return response.IndexOf("\"ok\":true") >= 0;
        }

        /// <summary>Asks the companion to open a TCP connection to the MCU acting as server.</summary>
        internal bool ConnectTo(string host, int port)
        {
            var response = SendCommand("{\"cmd\":\"connect_to\",\"host\":\"" + host + "\",\"port\":" + port + "}");
            return response.IndexOf("\"ok\":true") >= 0;
        }

        /// <summary>Asks the companion to start a TLS echo server on the given port.</summary>
        internal bool StartTlsEcho(int port)
        {
            var response = SendCommand("{\"cmd\":\"start_tls_echo\",\"port\":" + port + "}");
            return response.IndexOf("\"ok\":true") >= 0;
        }

        /// <summary>Asks the companion to open a TLS connection to the MCU acting as TLS server.</summary>
        internal bool TlsConnectTo(string host, int port)
        {
            var response = SendCommand("{\"cmd\":\"tls_connect_to\",\"host\":\"" + host + "\",\"port\":" + port + "}");
            return response.IndexOf("\"ok\":true") >= 0;
        }

        /// <summary>
        /// Asks the companion to open a TLS connection to the MCU, send the given data, and
        /// read back the echo. The TLS handshake and data exchange happen in the background
        /// on the companion; this returns true as soon as the TCP connection is established
        /// (the device drives the echo and verifies the payload on its side).
        /// </summary>
        internal bool TlsConnectEcho(string host, int port, byte[] data)
        {
            string dataB64 = Convert.ToBase64String(data);
            var resp = SendCommand("{\"cmd\":\"tls_connect_echo\",\"host\":\"" + host + "\",\"port\":" + port + ",\"data\":\"" + dataB64 + "\"}");

            return resp.IndexOf("\"ok\":true") >= 0;
        }

        private string SendCommand(string json)
        {
            byte[] cmd = Encoding.UTF8.GetBytes(json + "\n");
            _socket.Send(cmd);

            // Give the companion time to act and respond
            Thread.Sleep(100);

            // Read until we get a newline (end of JSON response)
            string result = "";
            int maxAttempts = 50;

            while (maxAttempts-- > 0)
            {
                int received = _socket.Receive(_recvBuf);
                if (received > 0)
                {
                    result += new string(Encoding.UTF8.GetChars(_recvBuf, 0, received));

                    if (result.IndexOf("\n") >= 0)
                    {
                        break;
                    }
                }

                Thread.Sleep(50);
            }

            return result;
        }

        public void Dispose() => _socket.Close();
    }
}
