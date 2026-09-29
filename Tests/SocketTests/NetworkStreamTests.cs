// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace NFUnitTestSocketTests
{
    [TestClass]
    public class NetworkStreamTests
    {
        private const int ServerPort = 7010;

        [Setup]
        public void SetupNetworkStreamTests()
        {
            // Comment next line to run the tests on a real hardware
            Assert.SkipTest("Skipping tests using nanoCLR Win32 in a pipeline");
        }

        [TestMethod]
        public void NetworkStream_Write_ByteArray_SendsData()
        {
            byte[] sent = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

            RunLoopbackTest((stream, peer) =>
            {
                stream.Write(sent, 0, sent.Length);

                AssertReceived(peer, sent);
            });
        }

        [TestMethod]
        public void NetworkStream_Write_ByteArrayWithOffset_SendsData()
        {
            byte[] buffer = new byte[] { 0xFF, 0x11, 0x22, 0x33, 0xFF };

            RunLoopbackTest((stream, peer) =>
            {
                stream.Write(buffer, 1, 3);

                AssertReceived(peer, new byte[] { 0x11, 0x22, 0x33 });
            });
        }

        [TestMethod]
        public void NetworkStream_Write_Span_SendsData()
        {
            byte[] sent = new byte[] { 0xAB, 0xCD, 0xEF };

            RunLoopbackTest((stream, peer) =>
            {
                stream.Write(new ReadOnlySpan<byte>(sent));

                AssertReceived(peer, sent);
            });
        }

        [TestMethod]
        public void NetworkStream_Write_ZeroLength_DoesNotThrow()
        {
            RunLoopbackTest((stream, peer) =>
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
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);

            try
            {
                testSockets.Startup(0, 0);

                // a connected datagram socket has a remote endpoint, so this exercises the socket type check
                testSockets.socketClient.Connect(testSockets.epServer);

                Assert.ThrowsException(typeof(IOException), () =>
                {
                    _ = new NetworkStream(testSockets.socketClient);
                });

                Assert.ThrowsException(typeof(IOException), () =>
                {
                    _ = new NetworkStream(testSockets.socketClient, false);
                });
            }
            finally
            {
                testSockets.TearDown();
            }
        }

        [TestMethod]
        public void Ctor_ConnectedStreamSocket_Succeeds()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Socket acceptedSocket = null;

            try
            {
                testSockets.Startup(0, 0);
                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                acceptedSocket = testSockets.socketServer.Accept();

                NetworkStream clientStream = new NetworkStream(testSockets.socketClient);
                NetworkStream serverStream = new NetworkStream(acceptedSocket, true);

                clientStream.Write(testSockets.bufSend, 0, testSockets.bufSend.Length);

                int bytesRead = serverStream.Read(testSockets.bufReceive, 0, testSockets.bufReceive.Length);

                testSockets.AssertDataReceived(bytesRead);

                clientStream.Dispose();

                // server stream owns the accepted socket, so disposing it closes the socket
                serverStream.Dispose();
                acceptedSocket = null;
            }
            finally
            {
                acceptedSocket?.Close();
                testSockets.TearDown();







            }
        }

        private delegate void LoopbackTestAction(NetworkStream stream, Socket peer);

        private static void RunLoopbackTest(LoopbackTestAction test)
        {
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket peer = null;
            NetworkStream stream = null;

            try
            {
                listener.Bind(new IPEndPoint(IPAddress.Loopback, ServerPort));
                listener.Listen(1);

                client.Connect(new IPEndPoint(IPAddress.Loopback, ServerPort));
                peer = listener.Accept();

                stream = new NetworkStream(client, true);

                test(stream, peer);
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
                    client.Close();
                }

                peer?.Close();
                listener.Close();
            }
        }

        private static void AssertReceived(Socket peer, byte[] expected)
        {
            byte[] received = new byte[expected.Length];
            int totalRead = 0;

            while (totalRead < expected.Length)
            {
                int read = peer.Receive(received, totalRead, expected.Length - totalRead, SocketFlags.None);

                Assert.IsTrue(read > 0, "Connection closed before all data was received");

                totalRead += read;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], received[i], $"Data mismatch at index {i}");
            }
        }
    }
}
