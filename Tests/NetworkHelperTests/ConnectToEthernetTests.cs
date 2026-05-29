using nanoFramework.Networking;
using nanoFramework.TestFramework;
using System;
using System.Threading;

namespace NetworkHelperTests
{
    [TestClass]
    public class ConnectToEthernetTests
    {
        [Setup]
        public void SetupConnectToEthernetTests()
        {
            // Comment next line to run the tests on a real hardware
            Assert.SkipTest("Skipping tests using nanoCLR Win32 in a pipeline");
        }

        [TestMethod]
        public void TestFixedIPAddress_01()
        {
            // wait 10 seconds to connect to the network
            CancellationTokenSource cs = new(10000);
            
            var success = NetworkHelper.SetupAndConnectNetwork(new IPConfiguration(
                "192.168.1.222",
                "255.255.255.0",
                "192.168.1.1",
                new[] { "192.168.1.1" }), requiresDateTime: true, token: cs.Token);

            DisplayLastError(success);
            
            Assert.IsTrue(success);

            NetworkHelper.Reset();
        }

        [TestMethod]
        public void TestFixedIPAddress_02()
        {
            NetworkHelper.SetupNetworkHelper(new IPConfiguration(
                "192.168.1.111",
                "255.255.255.0",
                "192.168.1.1",
                new[] { "192.168.1.1" }), true);

            // wait 10 seconds to connect to the network
            Assert.IsTrue(NetworkHelper.NetworkReady.WaitOne(10000, true));

            NetworkHelper.Reset();
        }

        [TestMethod]
        public void TestDhcp_01()
        {
            // wait 10 seconds to connect to the network and get an IP address
            CancellationTokenSource cs = new(10000);
            var success = NetworkHelper.SetupAndConnectNetwork(
                requiresDateTime: true,
                token: cs.Token);

            DisplayLastError(success);

            Assert.IsTrue(success);

            NetworkHelper.Reset();
        }

        [TestMethod]
        public void TestDhcp_02()
        {
            NetworkHelper.SetupNetworkHelper(true);

            // wait 10 seconds to connect to the network and get an IP address
            Assert.IsTrue(NetworkHelper.NetworkReady.WaitOne(10000, true));

            NetworkHelper.Reset();
        }

        [TestMethod]
        public void TestSingleUsage()
        {
            Assert.ThrowsException(typeof(InvalidOperationException), () =>
            {
                // call once, it's OK
                NetworkHelper.SetupNetworkHelper();

               // call twice without Reset — must throw
                NetworkHelper.SetupNetworkHelper();
            });

            NetworkHelper.Reset();
        }

        [TestMethod]
        public void TestRetryAfterTimeout()
        {
            // First attempt: very short timeout so it expires
            CancellationTokenSource cs1 = new(1000);
            var firstResult = NetworkHelper.SetupAndConnectNetwork(token: cs1.Token);

            Assert.IsFalse(firstResult, "First call should have timed out");
            Assert.IsTrue(NetworkHelper.Status == NetworkHelperStatus.TokenExpiredWaitingIPAddress);

            // Second attempt: longer timeout — must not throw InvalidOperationException
            CancellationTokenSource cs2 = new(10000);
            var secondResult = NetworkHelper.SetupAndConnectNetwork(token: cs2.Token);

            // If there is a network, second attempt should succeed;
            // if not, it will time out again — either way, it must NOT throw
            Assert.IsTrue(NetworkHelper.Status != NetworkHelperStatus.None);

            NetworkHelper.Reset();
        }

        [TestMethod]
        public void TestResetAllowsSetupNetworkHelperRestart()
        {
            NetworkHelper.SetupNetworkHelper();

            // Reset and call again — must not throw
            NetworkHelper.Reset();
            NetworkHelper.SetupNetworkHelper();

            // wait briefly
            NetworkHelper.NetworkReady.WaitOne(5000, true);

            NetworkHelper.Reset();
        }

        public void DisplayLastError(bool success)
        {
            if (success)
            {
                OutputHelper.WriteLine("Connection to network was successful");
            }
            else
            {
                OutputHelper.WriteLine($"Failed to connect to network, status is: {NetworkHelper.Status}");
            }
        }
    }
}
