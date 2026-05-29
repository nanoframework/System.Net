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
        public void Setup()
        {
            // Comment next line to run the tests on a real hardware
            Assert.SkipTest("Skipping tests using nanoCLR Win32 in a pipeline");
        }

        [TestMethod]
        public void NetTest1_DNS()
        {
            IPHostEntry ipHostEntry = Dns.GetHostEntry("192.168.1.1");

            Assert.AreEqual(1, ipHostEntry.AddressList.Length, "GetHostEntry returned wrong number of addresses");
            IPAddress address = ipHostEntry.AddressList[0];

            Assert.IsNotNull(address, "Address is null");
            Assert.AreEqual("192.168.1.1", address.ToString(), "Address is incorrect");
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
                Assert.IsNotNull(address, "Address is null");

                Type typeOfAddress = address.GetType();
                Assert.IsInstanceOfType(address, Type.GetType("System.Net.IPAddress"), "Type is incorrect");

                byte[] targetBytes = { (byte)IPInts[0], (byte)IPInts[1], (byte)IPInts[2], (byte)IPInts[3] };
                byte[] addressBytes = address.GetAddressBytes();
                Assert.AreEqual(4, addressBytes.Length, "GetAddressBytes returns wrong size");

                for (int j = 0; j < 4; j++)
                {
                    Assert.AreEqual(targetBytes[j], addressBytes[j], "GetAddressBytes returns wrong bytes");
                }

                IPAddress addressFromByteArray = new(targetBytes);
                addressBytes = addressFromByteArray.GetAddressBytes();
                for (int j = 0; j < 4; j++)
                {
                    Assert.AreEqual(targetBytes[j], addressBytes[j], "Address from byte array returns wrong bytes");
                }

                IPAddress address2 = new(
                    IPInts[0]
                    + IPInts[1] * 256
                    + IPInts[2] * 256 * 256
                    + IPInts[3] * 256 * 256 * 256);

                Assert.AreEqual(address.ToString(), address2.ToString(), "ToString returns differently for same data");

                Assert.AreEqual(address.GetHashCode(), address2.GetHashCode(), "GetHasCode returns differently for same data");

                address2 = new IPAddress(
                    (IPInts[0] % 2 + 1)
                    + (IPInts[1] % 2 + 1) * 256
                    + (IPInts[2] % 2 + 1) * 256 * 256
                    + (IPInts[3] % 2 + 1) * 256 * 256 * 256);

                Assert.AreNotEqual(address.GetHashCode(), address2.GetHashCode(), "GetHasCode returns same for " + address.ToString()
                        + " as " + address2.ToString());
            }
        }

        [TestMethod]
        public void NetTest3_IPAddressLoopback()
        {
            IPAddress address = IPAddress.Loopback;
            Assert.IsNotNull(address, "Address is null");

            Assert.AreEqual("127.0.0.1", address.ToString(), "Address is incorrect");

            Type typeOfAddress = address.GetType();
            Assert.IsInstanceOfType(address, Type.GetType("System.Net.IPAddress"), "Type is incorrect");

            byte[] localhostBytes = { 127, 0, 0, 1 };
            byte[] addressBytes = address.GetAddressBytes();
            Assert.AreEqual(4, addressBytes.Length, "GetAddressBytes returns wrong size");

            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(localhostBytes[i], addressBytes[i], "GetAddressBytes returns wrong bytes");
            }
        }


        [TestMethod]
        public void NetTest4_IPAddressAny()
        {
            IPAddress address = IPAddress.Any;

            Assert.IsNotNull(address, "Address is null");

            Assert.AreEqual("0.0.0.0", address.ToString(), "Address is incorrect");

            Type typeOfAddress = address.GetType();
            Assert.IsInstanceOfType(address, Type.GetType("System.Net.IPAddress"), "Type is incorrect");

            byte[] localhostBytes = { 0, 0, 0, 0 };
            byte[] addressBytes = address.GetAddressBytes();
            Assert.AreEqual(4, addressBytes.Length, "GetAddressBytes returns wrong size");

            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(localhostBytes[i], addressBytes[i], "GetAddressBytes returns wrong bytes");
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

                Assert.IsNotNull(endPoint1, "EndPoint1 is null");
                Assert.IsNotNull(endPoint2, "EndPoint2 is null");

                Type typeOfEndPoint = endPoint1.GetType();
                Assert.IsInstanceOfType(endPoint1, Type.GetType("System.Net.IPEndPoint"), "EndPoint1 Type is incorrect");

                typeOfEndPoint = endPoint2.GetType();
                Assert.IsInstanceOfType(endPoint2, Type.GetType("System.Net.IPEndPoint"), "EndPoint2 Type is incorrect");

                Assert.AreEqual(endPoint1.ToString(), endPoint2.ToString(), "ToString returns differently for same data");

                Assert.IsTrue(endPoint1.Equals(endPoint2), "Equals returns false for same data");

                int hashCode1 = endPoint1.GetHashCode();
                int hashCode2 = endPoint2.GetHashCode();

                Assert.AreEqual(hashCode1, hashCode2, "GetHasCode returns differently for same data");

                Assert.IsFalse(endPoint1.Address.ToString() != endPoint2.Address.ToString()
                    || endPoint1.Address.ToString() != address.ToString()
                    || endPoint2.Address.ToString() != address.ToString(), "Address returns wrong data");

                Assert.IsFalse(endPoint1.Port != endPoint2.Port
                    || endPoint1.Port != portInt
                    || endPoint2.Port != portInt, "Port returns wrong data");

                Debug.WriteLine("Cloning Enpoint1 into EndPoint2");
                endPoint2 = (IPEndPoint)endPoint2.Create(endPoint1.Serialize());

                typeOfEndPoint = endPoint2.GetType();
                Assert.IsInstanceOfType(endPoint2, Type.GetType("System.Net.IPEndPoint"), "EndPoint2 Type is incorrect after clone");

                Assert.AreEqual(endPoint1.ToString(), endPoint2.ToString(), "ToString returns differently for cloned data");

                Assert.AreEqual(endPoint1.GetHashCode(), endPoint2.GetHashCode(), "GetHashCode returns differently for cloned data");

                Assert.IsFalse(endPoint1.Address.ToString() != endPoint2.Address.ToString()
                    || endPoint1.Address.ToString() != address.ToString()
                    || endPoint2.Address.ToString() != address.ToString(), "Address returns wrong data after clone");

                Assert.IsFalse(endPoint1.Port != endPoint2.Port
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

                Assert.AreNotEqual(endPoint1.GetHashCode(), endPoint2.GetHashCode(), "GetHashCode returns same for "
                        + endPoint1.ToString()
                        + " as " + endPoint2.ToString());

                Assert.IsFalse(endPoint1.Address == endPoint2.Address
                    || endPoint2.Address == address, "Address returns wrong data after change");

                Assert.IsFalse(endPoint1.Port == endPoint2.Port
                    || endPoint2.Port == portInt, "Port returns wrong data after change");
            }
        }

        [TestMethod]
        public void NetTest5_IPHostEntryBasic()
        {
            IPHostEntry ipHostEntry = Dns.GetHostEntry("192.168.1.1");
            Assert.IsNotNull(ipHostEntry, "IPHostEntry is null");

            Type typeOfIPHostEntry = ipHostEntry.GetType();
            Assert.IsInstanceOfType(ipHostEntry, Type.GetType("System.Net.IPHostEntry"), "IPHostEntry Type is incorrect");

            Assert.AreEqual("192.168.1.1", ipHostEntry.AddressList[0].ToString(), "AddressList[0] is incorrect");
            Assert.ThrowsException(typeof(IndexOutOfRangeException), () => { ipHostEntry.AddressList[1].ToString(); });
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

                Assert.IsNotNull(socketAddress1, "socketAddress1 is null");
                Assert.IsNotNull(socketAddress2, "socketAddress2 is null");

                Type typeOfSocketAddress = socketAddress1.GetType();
                Assert.IsInstanceOfType(socketAddress1, Type.GetType("System.Net.SocketAddress"), "socketAddress1 Type is incorrect");

                typeOfSocketAddress = socketAddress2.GetType();
                Assert.IsInstanceOfType(socketAddress2, Type.GetType("System.Net.SocketAddress"), "socketAddress2 Type is incorrect");

                Assert.AreEqual(socketAddress1.ToString(), socketAddress2.ToString(), "ToString returns differently for same data");

                Assert.AreEqual(socketAddress1.GetHashCode(), socketAddress2.GetHashCode(), $"GetHashCode returns differently for same data");
                Assert.IsTrue(socketAddress1.Family == AddressFamily.InterNetwork, "socketAddress1 Family is incorrect");
                Assert.IsTrue(socketAddress2.Family == AddressFamily.InterNetwork, "socketAddress2 Family is incorrect");

                // Recreate a different Socket
                socketAddress2 = new SocketAddress(AddressFamily.Chaos, 8);

                Assert.AreNotEqual(socketAddress1.GetHashCode(), socketAddress2.GetHashCode(), "GetHashCode returns same for "
                        + socketAddress1.ToString() + " " + socketAddress1.GetHashCode()
                        + " as " + socketAddress2.ToString() + " " + socketAddress2.GetHashCode());
            }
        }

        [TestMethod]
        public void Equality_Tests()
        {
            var privateAddress = IPAddress.Parse("192.168.0.1");
            var publicAddress = IPAddress.Parse("1.1.1.1");
            IPAddress defaultAddress = default;
            IPAddress nullAddress = null;

            // Is
            Assert.IsTrue(nullAddress is null, "nullAddress is null");

            // Equal
            Assert.AreEqual(privateAddress, IPAddress.Parse("192.168.0.1"));
            Assert.AreEqual(publicAddress, IPAddress.Parse("1.1.1.1"));
            Assert.IsTrue(privateAddress == new IPAddress(new byte[] { 192, 168, 0, 1 }), "192.168.0.1 == 192.168.0.1");
            Assert.IsTrue(publicAddress == new IPAddress(new byte[] { 1, 1, 1, 1 }), "1.1.1.1 == 1.1.1.1");
            Assert.IsTrue(defaultAddress == default, "default == default");
            Assert.IsTrue(nullAddress == null, "nullAddress == null");

            // Not Equal
            Assert.AreNotEqual(privateAddress, IPAddress.Parse("1.1.1.1"));
            Assert.AreNotEqual(publicAddress, IPAddress.Parse("192.168.0.1"));
            Assert.IsTrue(privateAddress != new IPAddress(new byte[] { 192, 168, 0, 2 }), "192.168.0.1 == 192.168.0.2");
            Assert.IsTrue(publicAddress != new IPAddress(new byte[] { 1, 1, 1, 2 }), "1.1.1.1 == 1.1.1.2");
            Assert.IsTrue((IPAddress) null != privateAddress, "(IPAddress) null != privateAddress");
        }
    }
}
