//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

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
        [Setup]
        public void SetupConnectToEthernetTests()
        {
            // Comment next line to run the tests on a real hardware
            Assert.SkipTest("Skipping tests using nanoCLR Win32 in a pipeline");
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
    }
}
