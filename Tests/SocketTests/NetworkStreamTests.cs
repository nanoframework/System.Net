// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.Networking;
using nanoFramework.TestFramework;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NFUnitTestSocketTests
{
    /// <summary>
    /// NetworkStream tests. The connected peer is the Network Test Companion's TCP echo server: loopback isn't
    /// available on the targets (lwIP builds have it disabled), so data written is verified by reading the echo back.
    /// </summary>
    [TestClass]
    public class NetworkStreamTests
    {
        private const int EchoPort = 7010;

        private static bool _networkInitialized = false;

        [Setup]
        public void SetupNetworkStreamTests()
        {
            // Comment next line to run the tests on a real hardware with the companion running
            Assert.SkipTest("Skipping tests using nanoCLR Win32 in a pipeline");

            CancellationTokenSource cs = new CancellationTokenSource(30000);

            if (!NetworkHelper.SetupAndConnectNetwork(requiresDateTime: false, token: cs.Token))
            {
                Assert.SkipTest($"Network not available ({NetworkHelper.Status}) - skipping NetworkStream tests");
            }

            _networkInitialized = true;

            CompanionClient.EnsureReachable();
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
        public void NetworkStream_Write_ByteArray_SendsData()
        {
            byte[] sent = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

            RunEchoTest((stream) =>
            {
                stream.Write(sent, 0, sent.Length);

                AssertEchoed(stream, sent);
            });
        }

        [TestMethod]
        public void NetworkStream_Write_ByteArrayWithOffset_SendsData()
        {
            byte[] buffer = new byte[] { 0xFF, 0x11, 0x22, 0x33, 0xFF };

            RunEchoTest((stream) =>
            {
                stream.Write(buffer, 1, 3);

                AssertEchoed(stream, new byte[] { 0x11, 0x22, 0x33 });
            });
        }

        [TestMethod]
        public void NetworkStream_Write_Span_SendsData()
        {
            byte[] sent = new byte[] { 0xAB, 0xCD, 0xEF };

            RunEchoTest((stream) =>
            {
                stream.Write(new ReadOnlySpan<byte>(sent));

                AssertEchoed(stream, sent);
            });
        }

        [TestMethod]
        public void NetworkStream_Write_ZeroLength_DoesNotThrow()
        {
            RunEchoTest((stream) =>
            {
                stream.Write(new byte[0], 0, 0);
            });
        }

        [TestMethod]
        public void Ctor_NullSocket_ThrowsArgumentNullException()
        {
            Assert.ThrowsException(typeof(ArgumentNullException), () =>
            {
                _ = new NetworkStream(null);
            });

            Assert.ThrowsException(typeof(ArgumentNullException), () =>
            {
                _ = new NetworkStream(null, true);

            });
        }

        [TestMethod]
        public void Ctor_UnconnectedStreamSocket_ThrowsIOException()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Assert.ThrowsException(typeof(IOException), () =>
                {
                    _ = new NetworkStream(socket);
                });

                Assert.ThrowsException(typeof(IOException), () =>
                {
                    _ = new NetworkStream(socket, false);
                });
            }
            finally
            {
                socket.Close();
            }
        }

        [TestMethod]
        public void Ctor_ConnectedDgramSocket_ThrowsIOException()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                // a connected datagram socket has a remote endpoint, so this exercises the socket type check.
                // UDP connect only records the remote endpoint, nothing needs to listen there.
                socket.Connect(new IPEndPoint(IPAddress.Parse(TestConfiguration.CompanionIP), EchoPort));

                Assert.ThrowsException(typeof(IOException), () =>
                {
                    _ = new NetworkStream(socket);
                });

                Assert.ThrowsException(typeof(IOException), () =>
                {
                    _ = new NetworkStream(socket, false);
                });
            }
            finally
            {
                socket.Close();
            }
        }

        [TestMethod]
        public void Ctor_ConnectedStreamSocket_Succeeds()
        {
            byte[] sent = new byte[] { 0x5A, 0xA5, 0x3C };

            using (var companion = new CompanionClient())
            {
                Assert.IsTrue(companion.StartTcpEcho(EchoPort), "Failed to start TCP echo");

                Socket client = null;

                try
                {
                    client = ConnectToEcho();

                    // doesn't own the socket: disposing it must leave the socket open
                    NetworkStream writeStream = new NetworkStream(client);
                    writeStream.Write(sent, 0, sent.Length);
                    writeStream.Dispose();

                    // owns the socket: reading the echo proves the socket survived the previous dispose
                    NetworkStream readStream = new NetworkStream(client, true);
                    AssertEchoed(readStream, sent);

                    // disposing it closes the socket
                    readStream.Dispose();
                    client = null;
                }
                finally
                {
                    client?.Close();
                    companion.Stop(EchoPort);
                }
            }
        }

        private delegate void EchoTestAction(NetworkStream stream);

        private static void RunEchoTest(EchoTestAction test)
        {
            using (var companion = new CompanionClient())
            {
                Assert.IsTrue(companion.StartTcpEcho(EchoPort), "Failed to start TCP echo");

                Socket client = null;
                NetworkStream stream = null;

                try
                {
                    client = ConnectToEcho();
                    stream = new NetworkStream(client, true);

                    test(stream);
                }
                finally
                {
                    if (stream != null)
                    {
                        // stream owns the client socket
                        stream.Close();
                    }
                    else
                    {
                        client?.Close();
                    }

                    companion.Stop(EchoPort);
                }
            }
        }

        private static Socket ConnectToEcho()
        {
            // give the companion time to bind the listener
            Thread.Sleep(100);

            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(new IPEndPoint(IPAddress.Parse(TestConfiguration.CompanionIP), EchoPort));

            return client;
        }

        private static void AssertEchoed(NetworkStream stream, byte[] expected)
        {
            byte[] received = new byte[expected.Length];
            int totalRead = 0;

            while (totalRead < expected.Length)
            {
                int read = stream.Read(received, totalRead, expected.Length - totalRead);

                Assert.IsTrue(read > 0, "Connection closed before all data was echoed");

                totalRead += read;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], received[i], $"Data mismatch at index {i}");
            }
        }
    }
}
