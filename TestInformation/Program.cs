using System;
using System.Threading;
using System.Net.NetworkInformation;


namespace TestInformation
{
    public class Program
    {
        public static void Main()
        {
            // change here the network interface index to be used during the test
            // usual values
            // 0: for STM32 boards (only one network adapter)
            // 2: for ESP32
            const int networkInterfaceIndex = 0;

            // change here the network configurations to be used during the test
            const string staticIPv4Address = "192.168.1.222";
            const string staticIPv4SubnetMask = "255.255.255.0";
            const string staticIPv4DNSAdress = "192.168.1.2";

            try
            {
                NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
                NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

                DisplayNetworkInterfacesDetails();

                // display wireless 802.11 network configurations
                Wireless80211Configuration[] configs = Wireless80211Configuration.GetAllWireless80211Configurations();
                Console.WriteLine("");
                Console.WriteLine("=====================");
                Console.WriteLine("Number of Wireless 802.11 configurations: " + configs.Length.ToString());
                int count = 0;
                foreach (Wireless80211Configuration conf in configs)
                {
                    Console.WriteLine("Number:" + count.ToString()); count++;
                    Console.WriteLine("ID: " + conf.Id);
                    Console.WriteLine("Authentication: " + conf.Authentication);
                    Console.WriteLine("Encryption: " + conf.Encryption);
                    Console.WriteLine("Radio: " + conf.Radio);
                    Console.WriteLine("SSID: " + conf.Ssid);
                    Console.WriteLine("Password: " + conf.Password);
                    Console.WriteLine("");
                }

                int loopCount = 0;

                // Loop displaying the current network information for Interfaces available 
                while (true)
                {
                    loopCount++;
                    // Test code for DHCP and Static adapter configs
                    // Switch every 20 loops between the two
                    if (loopCount % 20 == 0)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Switching to DHCP");
                        Console.WriteLine("");

                        // switch to DHCP, get test interface
                        NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[networkInterfaceIndex];
                        ni.EnableAutomaticDns();
                        ni.EnableDhcp();
                    }
                    else if (loopCount % 10 == 0)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("Switching to static IPv4");
                        Console.WriteLine("");

                        // switch to static address, get test interface
                        NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[networkInterfaceIndex];
                        ni.EnableStaticIPv4(staticIPv4Address, staticIPv4SubnetMask, staticIPv4DNSAdress);
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                // Do whatever please you with the exception caught
                Console.WriteLine("Exception:" + ex.Message);
                
            }
        }

        private static void DisplayNetworkInterfacesDetails()
        {
            // display network information for all available network interfaces
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            Console.WriteLine("");
            Console.WriteLine("=====================");
            Console.WriteLine("Number of interfaces: " + interfaces.Length.ToString());
            int count = 0;
            foreach (NetworkInterface ni in interfaces)
            {
                Console.WriteLine("Number:" + count.ToString()); count++;
                Console.WriteLine("IPv4: " + ni.IPv4Address + " Mask: " + ni.IPv4SubnetMask + " Gateway: " + ni.IPv4GatewayAddress);
                Console.WriteLine("DHCP enabled: " + ni.IsDhcpEnabled);
                Console.WriteLine("Automatic DNS enabled: " + ni.IsAutomaticDnsEnabled);
                Console.WriteLine("NetworkInterfaceType: " + ni.NetworkInterfaceType.ToString());
                Console.WriteLine("PhysicalAddress: " + BytesToHexString(ni.PhysicalAddress));
                foreach (string dnsa in ni.IPv4DnsAddresses)
                {
                    Console.WriteLine("IPv4 DNS address: " + dnsa);
                }
                Console.WriteLine("");
            }
        }

        private static string BytesToHexString( byte[] bytes)
        {
            string hex = "0123456789ABCDEF";
            string hexString = "";

            foreach( byte b in bytes)
            {
                hexString += hex[b>>4];
                hexString += hex[b & 0x0f];
            }

            return hexString;
        }

        private static void NetworkChange_NetworkAddressChanged(object sender, nanoFramework.Runtime.Events.EventArgs e)
        {
            Console.WriteLine("NetworkChange_NetworkAddressChanged");
            if (sender==null) Console.WriteLine("NetworkChange_NetworkAddressChanged sender null");

            DisplayNetworkInterfacesDetails();
        }

        private static void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            Console.WriteLine("NetworkChange_NetworkAvailabilityChanged:"+e.IsAvailable.ToString());
            if (sender == null) Console.WriteLine("NetworkChange_NetworkAvailabilityChanged sender null");

            DisplayNetworkInterfacesDetails();
        }
    }
}
