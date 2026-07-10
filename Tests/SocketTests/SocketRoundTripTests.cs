//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Networking;
using nanoFramework.TestFramework;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace NFUnitTestSocketTests
{
    /// <summary>
    /// Round-trip socket tests that require the Network Test Companion to be running
    /// on the host PC. Start it with:
    ///   dotnet run --project Tests/NetworkTestCompanion
    /// and update TestConfiguration.CompanionIP when testing on real hardware.
    /// </summary>
    [TestClass]
    public class SocketRoundTripTests
    {
        private static bool _networkInitialized = false;

        [Setup]
        public void Setup()
        {
            // Comment the next line to run these tests on real hardware with the companion running
            Assert.SkipTest("Skipping round-trip tests: companion required - run on real hardware only");

            // Bring up the network before any socket operation.
            CancellationTokenSource cs = new CancellationTokenSource(30000);
            bool connected = NetworkHelper.SetupAndConnectNetwork(requiresDateTime: false, token: cs.Token);

            if (!connected)
            {
                Assert.SkipTest($"Network not available ({NetworkHelper.Status}) - skipping round-trip tests");
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
        public void RoundTrip_Tcp_SendReceive_Echo()
        {
            const int echoPort = 7001;

            using (var companion = new CompanionClient())
            {
                Assert.IsTrue(companion.Ping(), "Companion not reachable");
                Assert.IsTrue(companion.StartTcpEcho(echoPort), "Failed to start TCP echo");

                // Give the companion time to bind the listener
                Thread.Sleep(100);

                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(new IPEndPoint(IPAddress.Parse(TestConfiguration.CompanionIP), echoPort));

                try
                {
                    byte[] sent = new byte[] { 0xAB, 0xCD, 0xEF };
                    client.Send(sent);

                    byte[] received = new byte[sent.Length];
                    int totalRead = 0;
                    while (totalRead < sent.Length)
                    {
                        int n = client.Receive(received, totalRead, sent.Length - totalRead, SocketFlags.None);
                        Assert.IsTrue(n > 0, "Connection closed before all bytes received");
                        totalRead += n;
                    }

                    Assert.IsTrue(SocketTools.ArrayEquals(sent, received), "Echoed bytes do not match sent bytes");
                }
                finally
                {
                    client.Close();
                    companion.Stop(echoPort);
                }
            }
        }

        [TestMethod]
        public void RoundTrip_Tcp_LargeBuffer_Echo()
        {
            const int echoPort = 7002;
            const int bufferSize = 1024;

            using (var companion = new CompanionClient())
            {
                Assert.IsTrue(companion.Ping(), "Companion not reachable");
                Assert.IsTrue(companion.StartTcpEcho(echoPort), "Failed to start TCP echo");

                Thread.Sleep(100);

                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(new IPEndPoint(IPAddress.Parse(TestConfiguration.CompanionIP), echoPort));

                try
                {
                    byte[] sent = new byte[bufferSize];

                    for (int i = 0; i < bufferSize; i++)
                    {
                        sent[i] = (byte)(i & 0xFF);
                    }

                    client.Send(sent);

                    byte[] received = new byte[bufferSize];
                    int totalRead = 0;
                    while (totalRead < bufferSize)
                    {
                        int n = client.Receive(received, totalRead, bufferSize - totalRead, SocketFlags.None);
                        Assert.IsTrue(n > 0, "Connection closed before all bytes received");
                        totalRead += n;
                    }

                    Assert.IsTrue(SocketTools.ArrayEquals(sent, received), "Large buffer echo mismatch");
                }
                finally
                {
                    client.Close();
                    companion.Stop(echoPort);
                }
            }
        }

        [TestMethod]
        public void RoundTrip_Udp_SendReceive_Echo()
        {
            const int echoPort = 7003;

            using (var companion = new CompanionClient())
            {
                Assert.IsTrue(companion.Ping(), "Companion not reachable");
                Assert.IsTrue(companion.StartUdpEcho(echoPort), "Failed to start UDP echo");

                Thread.Sleep(100);

                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                client.Bind(new IPEndPoint(IPAddress.Any, 0));

                try
                {
                    EndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(TestConfiguration.CompanionIP), echoPort);

                    byte[] sent = new byte[] { 0x11, 0x22, 0x33 };
                    client.SendTo(sent, serverEndPoint);

                    byte[] received = new byte[16];
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int n = client.ReceiveFrom(received, ref remoteEndPoint);

                    Assert.AreEqual(sent.Length, n, "UDP echo returned wrong byte count");
                    for (int i = 0; i < sent.Length; i++)
                    {
                        Assert.AreEqual(sent[i], received[i], $"UDP echo mismatch at byte {i}");
                    }
                }
                finally
                {
                    client.Close();
                    companion.Stop(echoPort);
                }
            }
        }

        [TestMethod]
        public void RoundTrip_Tcp_McuAsServer_CompanionConnects()
        {
            const int mcuListenPort = 7004;

            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Any, mcuListenPort));
            server.Listen(1);

            try
            {
                using (var companion = new CompanionClient())
                {
                    Assert.IsTrue(companion.Ping(), "Companion not reachable");

                    string mcuIp = GetLocalIp();
                    Assert.IsTrue(companion.ConnectTo(mcuIp, mcuListenPort), "ConnectTo command failed");

                    Assert.IsTrue(server.Poll(10 * 1000 * 1000, SelectMode.SelectRead), "No connection received from companion within timeout");
                    Socket accepted = server.Accept();

                    try
                    {
                        Assert.IsNotNull(accepted, "Accept returned null");
                    }
                    finally
                    {
                        accepted.Close();
                    }
                }
            }
            finally
            {
                server.Close();
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

                // Skip loopback, disconnected, and virtual (link-local) addresses.
                if (ni.IPv4Address.StartsWith("127.") || ni.IPv4Address.StartsWith("169.254."))
                {
                    continue;
                }

                return ni.IPv4Address;
            }

            return "0.0.0.0";
        }
    }
}
