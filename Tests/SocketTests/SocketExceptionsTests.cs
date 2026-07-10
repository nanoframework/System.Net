//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.TestFramework;
using System;
using System.Net;
using System.Net.Sockets;

namespace NFUnitTestSocketTests
{
    [TestClass]
    public class SocketExceptionsTests
    {
        [Setup]
        public void SetupConnectToEthernetTests()
        {
            // Comment next line to run the tests on a real hardware
            Assert.SkipTest("Skipping tests using nanoCLR Win32 in a pipeline");
        }

        [TestMethod]
        public void SocketExceptionTest2_AddressAlreadyInUse()
        {
            Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Assert.ThrowsException(typeof(SocketException), () =>
            {

                socketClient.Bind(new IPEndPoint(IPAddress.Loopback, 10));
                socketServer.Bind(new IPEndPoint(IPAddress.Loopback, 10));
            });

            socketClient?.Close();
            socketServer?.Close();
        }

        [TestMethod]
        public void SocketExceptionTest3_Protocol_Address_FamilyNotSupported()
        {
            try
            {
                Socket socketTest = new Socket(AddressFamily.AppleTalk,
                    SocketType.Stream, ProtocolType.Udp);
            }
            catch (SocketException e)
            {
                Assert.IsFalse(e.ErrorCode != (int)SocketError.ProtocolFamilyNotSupported && e.ErrorCode != (int)SocketError.AddressFamilyNotSupported && e.ErrorCode != (int)SocketError.ProtocolNotSupported, "Incorrect ErrorCode in SocketException "
                        + e.ErrorCode);
                return;
            }
            throw new Exception("No SocketException thrown");
        }


        [TestMethod]
        public void SocketExceptionTest4_ProtocolNotSupported()
        {
            Assert.ThrowsException(typeof(SocketException), () =>
            {
                Socket socketTest = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Udp);
            });
        }

        [TestMethod]
        public void SocketExceptionTest6_IsConnected()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);

            Assert.ThrowsException(typeof(SocketException), () =>
            {
                testSockets.Startup(0, 0);
                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                testSockets.socketClient.Connect(testSockets.epServer);
            });

            testSockets.TearDown();
        }


        [TestMethod]
        public void SocketExceptionTest11_AccessDenied()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Dgram);
            Assert.ThrowsException(typeof(SocketException), () =>
            {
                int clientPort = SocketTools.nextPort;
                int serverPort = SocketTools.nextPort;
                int tempPort = serverPort;
                testSockets.socketClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
                testSockets.Startup(clientPort, serverPort);
                IPEndPoint epBroadcast = new IPEndPoint(SocketTools.DottedDecimalToIp((byte)255, (byte)255, (byte)255, (byte)255), tempPort);
                EndPoint serverEndPoint = epBroadcast.Create(epBroadcast.Serialize());
                testSockets.socketClient.SendTo(testSockets.bufSend, serverEndPoint);
            });

            testSockets.TearDown();
        }

        [TestMethod]
        public void SocketExceptionTest12_NotConnected()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Socket socketTemp = null;

            Assert.ThrowsException(typeof(SocketException), () =>
            {
                testSockets.Startup(0, 0);
                socketTemp = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                socketTemp.Bind(testSockets.socketServer.RemoteEndPoint);
                socketTemp.Send(new byte[2]);
            });

            socketTemp?.Close();
            testSockets.TearDown();
        }

        [TestMethod]
        public void SocketExceptionTest13_InvalidArgument()
        {

            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Assert.ThrowsException(typeof(SocketException), () =>
            {
                int clientPort = SocketTools.nextPort;
                int serverPort = SocketTools.nextPort;
                int tempPort = clientPort;
                testSockets.Startup(clientPort, serverPort);
                testSockets.socketServer.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.Broadcast, true);
            });

            testSockets.TearDown();
        }


        [TestMethod]
        public void SocketExceptionTest14_AddressNotAvailable()
        {
            // Bind to an IP not assigned to any local interface, then connect to force address validation.
            // nanoFramework's LWIP stack defers the AddressNotAvailable error to connect time, not bind time.
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Socket freshSocket = null;

            Assert.ThrowsException(typeof(SocketException), () =>
            {
                int serverPort = SocketTools.nextPort;
                int clientPort = SocketTools.nextPort;
                testSockets.Startup(clientPort, serverPort);
                testSockets.socketServer.Listen(1);

                freshSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                freshSocket.Bind(new IPEndPoint(new IPAddress(SocketTools.DottedDecimalToIp((byte)192, (byte)168, (byte)192, (byte)168)), SocketTools.nextPort));
                freshSocket.Connect(testSockets.epServer);
            });

            freshSocket?.Close();
            testSockets.TearDown();
        }

        [TestMethod]
        public void SocketExceptionTest16_HostNotFound()
        {
            Assert.ThrowsException(typeof(SocketException), () =>
            {
                IPHostEntry ipHostEntry = Dns.GetHostEntry("fakeHostName");
            });
        }

        [TestMethod]
        public void SocketExceptionTest17_SocketError()
        {
            Assert.ThrowsException(typeof(SocketException), () =>
            {
                SocketPair testSockets = new SocketPair(ProtocolType.Udp, SocketType.Stream);
            });
        }

        [TestMethod]
        public void SocketExceptionTest18_Fault()
        {

            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Assert.ThrowsException(typeof(SocketException), () =>
            {
                testSockets.socketClient.SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.Linger, new byte[] { (byte)0 });
                testSockets.Startup(0, 0);
            });

            testSockets.TearDown();
        }

        [TestMethod]
        public void SocketExceptionTest19_ProtocolOption()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Assert.ThrowsException(typeof(SocketException), () =>
            {
                testSockets.Startup(0, 0);
                testSockets.socketClient.GetSocketOption(SocketOptionLevel.IP,
                    SocketOptionName.Linger);
            });
            testSockets.TearDown();
            testSockets = null;
        }


        [TestMethod]
        public void SocketExceptionTest20_OperationNotSupported()
        {
            SocketPair testSockets = new SocketPair(ProtocolType.Tcp, SocketType.Stream);
            Assert.ThrowsException(typeof(SocketException), () =>
            {
                testSockets.Startup(0, 0);

                testSockets.socketServer.Listen(1);
                testSockets.socketClient.Connect(testSockets.epServer);
                testSockets.socketClient.Send(testSockets.bufSend);

                using (Socket sock = testSockets.socketServer.Accept())
                {
                    sock.Receive(testSockets.bufReceive, SocketFlags.DontRoute);
                }
            });
            testSockets.TearDown();
            testSockets = null;
        }

    }
}
