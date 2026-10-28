//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Networking;
using nanoFramework.TestFramework;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NFUnitTestSocketTests
{
    /// <summary>
    /// Tests that need a connected TCP peer use the Network Test Companion's TCP echo server: loopback isn't
    /// available on the targets (lwIP builds have it disabled).
    /// </summary>
    [TestClass]
    public class SocketExceptionsTests
    {
        private static bool _networkInitialized = false;

        [Setup]
        public void SetupConnectToEthernetTests()
        {
            // Comment next line to run the tests on a real hardware with the companion running
            Assert.SkipTest("Skipping tests using nanoCLR Win32 in a pipeline");

            CancellationTokenSource cs = new CancellationTokenSource(30000);

            if (!NetworkHelper.SetupAndConnectNetwork(requiresDateTime: false, token: cs.Token))
            {
                Assert.SkipTest($"Network not available ({NetworkHelper.Status}) - skipping socket exception tests");
            }

            _networkInitialized = true;
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
        public void SocketExceptionTest2_AddressAlreadyInUse()
        {
            Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socketClient.Bind(new IPEndPoint(IPAddress.Any, 0));
                int port = ((IPEndPoint)socketClient.LocalEndPoint).Port;

                Assert.ThrowsException(typeof(SocketException), () =>
                {
                    socketServer.Bind(new IPEndPoint(IPAddress.Any, port));
                });
            }
            finally
            {
                socketClient.Close();
                socketServer.Close();
            }
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
            try
            {
                Socket socketTest = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Udp);
            }
            catch (SocketException e)
            {
                Assert.AreEqual((int)SocketError.ProtocolNotSupported, e.ErrorCode, "Incorrect ErrorCode in SocketException " + e.ErrorCode);
                return;
            }
            throw new Exception("No SocketException thrown");
        }

        [TestMethod]
        public void SocketExceptionTest6_IsConnected()
        {
            const int echoPort = 7011;

            RunWithCompanionEcho(echoPort, () =>
            {
                Socket client = ConnectToCompanion(echoPort);

                try
                {
                    Assert.ThrowsException(typeof(SocketException), () =>
                    {
                        client.Connect(new IPEndPoint(IPAddress.Parse(TestConfiguration.CompanionIP), echoPort));
                    });
                }
                finally
                {
                    client.Close();
                }
            });
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
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Assert.ThrowsException(typeof(SocketException), () =>
                {
                    socket.Send(new byte[2]);
                });
            }
            finally
            {
                socket.Close();
            }
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

        private static void RunWithCompanionEcho(int echoPort, Action test)
        {
            using (var companion = new CompanionClient())
            {
                try
                {
                    Assert.IsTrue(companion.StartTcpEcho(echoPort), "Failed to start TCP echo");

                    // give the companion time to bind the listener
                    Thread.Sleep(100);

                    test();
                }
                finally
                {
                    companion.Stop(echoPort);
                }
            }
        }

        private static Socket ConnectToCompanion(int port)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse(TestConfiguration.CompanionIP), port));

            return socket;
        }
    }
}
