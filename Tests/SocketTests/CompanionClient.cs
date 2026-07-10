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
        private readonly byte[] _recvBuf = new byte[128];

        internal CompanionClient()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(new IPEndPoint(
                IPAddress.Parse(TestConfiguration.CompanionIP),
                TestConfiguration.CompanionControlPort));

            // Brief settle time so the companion is ready to read
            Thread.Sleep(50);
        }

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

        private string SendCommand(string json)
        {
            byte[] cmd = Encoding.UTF8.GetBytes(json + "\n");
            _socket.Send(cmd);

            // Give the companion time to act and respond
            Thread.Sleep(100);

            int received = _socket.Receive(_recvBuf);
            return new string(Encoding.UTF8.GetChars(_recvBuf, 0, received));
        }

        public void Dispose() => _socket.Close();
    }
}
