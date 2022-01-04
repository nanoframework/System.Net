//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using nanoFramework.TestFramework;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NFUnitTestIPAddress
{
    [TestClass]
    public class IPAddressTests
    {
        [Setup]
        public void SetupConnectToEthernetTests()
        {
            // Comment next line to run the tests on a real hardware
            Assert.SkipTest("Skipping tests using nanoCLR Win32 in a pipeline");
        }

        [TestMethod]
        public void NetTest1_DNS()
        {
            IPHostEntry ipHostEntry = Dns.GetHostEntry("192.168.1.1");

            Assert.Equal(ipHostEntry.AddressList.Length, 1, "GetHostEntry returned wrong number of addresses");
            IPAddress address = ipHostEntry.AddressList[0];

            Assert.NotNull(address, "Address is null");
            Assert.Equal(address.ToString(), "192.168.1.1", "Address is incorrect");
        }

        [TestMethod]
        public void NetTest2_IPAddressBasic()
        {
            Random random = new();

            for (int i = 0; i <= 30; i++)
            {
                int[] IPInts = { 
                    random.Next(256), 
                    random.Next(256),
                    random.Next(256), 
                    random.Next(128) };

                Debug.WriteLine($"Random IP {IPInts[0]}.{IPInts[1]}.{IPInts[2]}.{IPInts[3]}");

                IPAddress address = new(
                    IPInts[0]
                    + IPInts[1] * 256
                    + IPInts[2] * 256 * 256
                    + IPInts[3] * 256 * 256 * 256);
                Assert.NotNull(address, "Address is null");

                Type typeOfAddress = address.GetType();
                Assert.IsType(typeOfAddress, Type.GetType("System.Net.IPAddress"), "Type is incorrect");

                byte[] targetBytes = { (byte)IPInts[0], (byte)IPInts[1], (byte)IPInts[2], (byte)IPInts[3] };
                byte[] addressBytes = address.GetAddressBytes();
                Assert.Equal(addressBytes.Length, 4, "GetAddressBytes returns wrong size");

                for (int j = 0; j < 4; j++)
                {
                    Assert.Equal(addressBytes[j], targetBytes[j], "GetAddressBytes returns wrong bytes");
                }

                IPAddress addressFromByteArray = new(targetBytes);
                addressBytes = addressFromByteArray.GetAddressBytes();
                for (int j = 0; j < 4; j++)
                {
                    Assert.Equal(addressBytes[j], targetBytes[j], "Address from byte array returns wrong bytes");
                }

                IPAddress address2 = new(
                    IPInts[0]
                    + IPInts[1] * 256
                    + IPInts[2] * 256 * 256
                    + IPInts[3] * 256 * 256 * 256);

                Assert.Equal(address.ToString(), address2.ToString(), "ToString returns differently for same data");

                Assert.Equal(address.GetHashCode(), address2.GetHashCode(), "GetHasCode returns differently for same data");

                address2 = new IPAddress(
                    (IPInts[0] % 2 + 1)
                    + (IPInts[1] % 2 + 1) * 256
                    + (IPInts[2] % 2 + 1) * 256 * 256
                    + (IPInts[3] % 2 + 1) * 256 * 256 * 256);

                Assert.NotEqual(address.GetHashCode(), address2.GetHashCode(), "GetHasCode returns same for " + address.ToString()
                        + " as " + address2.ToString());
            }
        }

        [TestMethod]
        public void NetTest3_IPAddressLoopback()
        {
            IPAddress address = IPAddress.Loopback;
            Assert.NotNull(address, "Address is null");

            Assert.Equal(address.ToString(), "127.0.0.1", "Address is incorrect");

            Type typeOfAddress = address.GetType();
            Assert.IsType(typeOfAddress, Type.GetType("System.Net.IPAddress"), "Type is incorrect");

            byte[] localhostBytes = { 127, 0, 0, 1 };
            byte[] addressBytes = address.GetAddressBytes();
            Assert.Equal(addressBytes.Length, 4, "GetAddressBytes returns wrong size");

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(addressBytes[i], localhostBytes[i], "GetAddressBytes returns wrong bytes");
            }
        }


        [TestMethod]
        public void NetTest4_IPAddressAny()
        {
            IPAddress address = IPAddress.Any;

            Assert.NotNull(address, "Address is null");

            Assert.Equal(address.ToString(), "0.0.0.0", "Address is incorrect");

            Type typeOfAddress = address.GetType();
            Assert.IsType(typeOfAddress, Type.GetType("System.Net.IPAddress"), "Type is incorrect");

            byte[] localhostBytes = { 0, 0, 0, 0 };
            byte[] addressBytes = address.GetAddressBytes();
            Assert.Equal(addressBytes.Length, 4, "GetAddressBytes returns wrong size");

            for (int i = 0; i < 4; i++)
            {
                Assert.Equal(addressBytes[i], localhostBytes[i], "GetAddressBytes returns wrong bytes");
            }
        }


        [TestMethod]
        public void NetTest5_IPEndPointBasic()
        {
            Random random = new();
            for (int i = 0; i <= 30; i++)
            {
                int[] IPInts = { 
                    random.Next(256),
                    random.Next(256),
                    random.Next(256),
                    random.Next(128) };

                int portInt = random.Next(65535) + 1;

                long addressLong =
                    IPInts[0]
                    + IPInts[1] * 256
                    + IPInts[2] * 256 * 256
                    + IPInts[3] * 256 * 256 * 256;

                Debug.WriteLine($"Random IP {IPInts[0]}.{IPInts[1]}.{IPInts[2]}.{IPInts[3]}:{portInt}");

                IPAddress address = new(addressLong);

                Debug.WriteLine("EndPoint1 created with IPAddress and int");
                IPEndPoint endPoint1 = new(address, portInt);

                Debug.WriteLine("EndPoint2 created with long and int");
                IPEndPoint endPoint2 = new(addressLong, portInt);

                Assert.NotNull(endPoint1, "EndPoint1 is null");
                Assert.NotNull(endPoint2, "EndPoint2 is null");

                Type typeOfEndPoint = endPoint1.GetType();
                Assert.IsType(typeOfEndPoint, Type.GetType("System.Net.IPEndPoint"), "EndPoint1 Type is incorrect");

                typeOfEndPoint = endPoint2.GetType();
                Assert.IsType(typeOfEndPoint, Type.GetType("System.Net.IPEndPoint"), "EndPoint2 Type is incorrect");

                Assert.Equal(endPoint1.ToString(), endPoint2.ToString(), "ToString returns differently for same data");

                Assert.True(endPoint1.Equals(endPoint2), "Equals returns false for same data");

                int hashCode1 = endPoint1.GetHashCode();
                int hashCode2 = endPoint2.GetHashCode();

                Assert.Equal(hashCode1, hashCode2, "GetHasCode returns differently for same data");

                Assert.False(endPoint1.Address.ToString() != endPoint2.Address.ToString()
                    || endPoint1.Address.ToString() != address.ToString()
                    || endPoint2.Address.ToString() != address.ToString(), "Address returns wrong data");

                Assert.False(endPoint1.Port != endPoint2.Port
                    || endPoint1.Port != portInt
                    || endPoint2.Port != portInt, "Port returns wrong data");

                Debug.WriteLine("Cloning Enpoint1 into EndPoint2");
                endPoint2 = (IPEndPoint)endPoint2.Create(endPoint1.Serialize());

                typeOfEndPoint = endPoint2.GetType();
                Assert.IsType(typeOfEndPoint, Type.GetType("System.Net.IPEndPoint"), "EndPoint2 Type is incorrect after clone");

                Assert.Equal(endPoint1.ToString(), endPoint2.ToString(), "ToString returns differently for cloned data");

                Assert.Equal(endPoint1.GetHashCode(), endPoint2.GetHashCode(), "GetHashCode returns differently for cloned data");

                Assert.False(endPoint1.Address.ToString() != endPoint2.Address.ToString()
                    || endPoint1.Address.ToString() != address.ToString()
                    || endPoint2.Address.ToString() != address.ToString(), "Address returns wrong data after clone");

                Assert.False(endPoint1.Port != endPoint2.Port
                    || endPoint1.Port != portInt
                    || endPoint2.Port != portInt, "Port returns wrong data after clone");

                Debug.WriteLine("Recreating EndPoint2 with new data");

                int portInt2 = portInt % 2 + 1;
                long addressLong2 =
                    (IPInts[0] % 2 + 1)
                    + (IPInts[1] % 2 + 1) * 256
                    + (IPInts[2] % 2 + 1) * 256 * 256
                    + (IPInts[3] % 2 + 1) * 256 * 256 * 256;
                endPoint2 = new IPEndPoint(addressLong2, portInt2);

                Assert.NotEqual(endPoint1.GetHashCode(), endPoint2.GetHashCode(), "GetHashCode returns same for "
                        + endPoint1.ToString()
                        + " as " + endPoint2.ToString());

                Assert.False(endPoint1.Address == endPoint2.Address
                    || endPoint2.Address == address, "Address returns wrong data after change");

                Assert.False(endPoint1.Port == endPoint2.Port
                    || endPoint2.Port == portInt, "Port returns wrong data after change");
            }
        }

        [TestMethod]
        public void NetTest5_IPHostEntryBasic()
        {
            IPHostEntry ipHostEntry = Dns.GetHostEntry("192.168.1.1");
            Assert.NotNull(ipHostEntry, "IPHostEntry is null");

            Type typeOfIPHostEntry = ipHostEntry.GetType();
            Assert.IsType(typeOfIPHostEntry, Type.GetType("System.Net.IPHostEntry"), "IPHostEntry Type is incorrect");

            Assert.Equal(ipHostEntry.AddressList[0].ToString(), "192.168.1.1", "AddressList[0] is incorrect");
            Assert.Throws(typeof(IndexOutOfRangeException), () => { ipHostEntry.AddressList[1].ToString(); });
        }

        [TestMethod]
        public void NetTest6_SocketAddressBasic()
        {
            Random random = new();

            for (int i = 0; i <= 30; i++)
            {
                int[] IPInts = {
                    random.Next(256),
                    random.Next(256),
                    random.Next(256), 
                    random.Next(128) };

                Debug.WriteLine($"Random IP {IPInts[0]}.{IPInts[1]}.{IPInts[2]}.{IPInts[3]}");

                IPAddress address = new(
                    IPInts[0]
                    + IPInts[1] * 256
                    + IPInts[2] * 256 * 256
                    + IPInts[3] * 256 * 256 * 256);

                int portInt = random.Next(65536);

                IPEndPoint ipEndpoint1 = new(address, portInt);

                SocketAddress socketAddress1 = ipEndpoint1.Serialize();
                SocketAddress socketAddress2 = ipEndpoint1.Serialize();

                Assert.NotNull(socketAddress1, "socketAddress1 is null");
                Assert.NotNull(socketAddress2, "socketAddress2 is null");

                Type typeOfSocketAddress = socketAddress1.GetType();
                Assert.IsType(typeOfSocketAddress, Type.GetType("System.Net.SocketAddress"), "socketAddress1 Type is incorrect");

                typeOfSocketAddress = socketAddress2.GetType();
                Assert.IsType(typeOfSocketAddress, Type.GetType("System.Net.SocketAddress"), "socketAddress2 Type is incorrect");

                Assert.Equal(socketAddress1.ToString(), socketAddress2.ToString(), "ToString returns differently for same data");

                Assert.Equal(socketAddress1.GetHashCode(), socketAddress2.GetHashCode(), $"GetHashCode returns differently for same data");
                Assert.True(socketAddress1.Family == AddressFamily.InterNetwork, "socketAddress1 Family is incorrect");
                Assert.True(socketAddress2.Family == AddressFamily.InterNetwork, "socketAddress2 Family is incorrect");

                // Recreate a different Socket
                socketAddress2 = new SocketAddress(AddressFamily.Chaos, 8);

                Assert.NotEqual(socketAddress1.GetHashCode(), socketAddress2.GetHashCode(), "GetHashCode returns same for "
                        + socketAddress1.ToString() + " " + socketAddress1.GetHashCode()
                        + " as " + socketAddress2.ToString() + " " + socketAddress2.GetHashCode());
            }
        }
    }
}
