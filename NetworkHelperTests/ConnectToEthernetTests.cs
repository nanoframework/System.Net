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
            
            Assert.True(success);

            // need to reset this internal flag to allow calling the NetworkHelper again
            NetworkHelper.ResetInstance();
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
            Assert.True(NetworkHelper.NetworkReady.WaitOne(10000, true));

            // need to reset this internal flag to allow calling the NetworkHelper again
            NetworkHelper.ResetInstance();
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

            Assert.True(success);

            // need to reset this internal flag to allow calling the NetworkHelper again
            NetworkHelper.ResetInstance();
        }

        [TestMethod]
        public void TestDhcp_02()
        {
            NetworkHelper.SetupNetworkHelper(true);

            // wait 10 seconds to connect to the network and get an IP address
            Assert.True(NetworkHelper.NetworkReady.WaitOne(10000, true));

            // need to reset this internal flag to allow calling the NetworkHelper again
            NetworkHelper.ResetInstance();
        }

        [TestMethod]
        public void TestSingleUsage()
        {
            Assert.Throws(typeof(InvalidOperationException), () =>
            {
                // call once, it's OK
                NetworkHelper.SetupNetworkHelper();

               // call twice, it's a NO NO and should throw an exception
                NetworkHelper.SetupNetworkHelper();
            });
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
