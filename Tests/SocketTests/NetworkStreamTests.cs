// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;
using System;
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
