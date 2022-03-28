//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.TestFramework;
using System;
using System.Net.Sockets;

namespace NFUnitTestSocketTests
{
    [TestClass]
    public class SocketOptionsTests
    {
        [Setup]
        public void SetupConnectToEthernetTests()
        {
            // Comment next line to run the tests on a real hardware
            Assert.SkipTest("Skipping tests using nanoCLR Win32 in a pipeline");
        }

        [TestMethod]
        public void SocketGetSocketOptions_00()
        {
            SocketType socketType = SocketType.Stream;

            Socket testSocket = new(
                AddressFamily.InterNetwork,
                socketType,
                ProtocolType.Tcp);

            Assert.Throws(typeof(NotSupportedException), () =>
            {
                testSocket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.AddMembership);
            }, "Getting SocketOptionName.AddMembership should have thrown an exception");

            Assert.Throws(typeof(NotSupportedException), () =>
            {
                testSocket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DropMembership);
            }, "Getting SocketOptionName.DropMembership should have thrown an exception");

            Assert.True((SocketType)testSocket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Type) == socketType, "Getting SocketOptionName.Type returned a different type.");

            testSocket?.Close();
        }

        [TestMethod]
        public void SocketLinger()
        {
            SocketType socketType = SocketType.Stream;

            Socket testSocket = new(
                AddressFamily.InterNetwork,
                socketType,
                ProtocolType.Tcp);

            // TODO
            // connect to endpoint

            // get linger option
            //Assert.IsType(typeof(bool), testSocket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger), "SocketOptionName.DontLinger should return a bool.");

            // set DontLinger option
            // read back linger option
            // set LINGER value

            // read back linger option

            testSocket?.Close();
        }
    }
}
