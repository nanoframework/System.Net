using System;
using System.Threading;
using System.Net.NetworkInformation;


namespace TestInformation
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
                NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

                int loopCount = 0;

                // Loop displaying the current betwork information for Interfaces available 
                while (true)
                {
                    NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                    Console.WriteLine("=====================");
                    Console.WriteLine("Number of interaces :" + interfaces.Length.ToString());
                    int count = 0;
                    foreach (NetworkInterface ni in interfaces)
                    {
                        Console.WriteLine("Number:" + count.ToString()); count++;
                        Console.WriteLine("IP-" + ni.IPAddress + " Mask:" + ni.SubnetMask + " Gateway" + ni.GatewayAddress);
                        Console.WriteLine("Dhcp enabled:" + ni.IsDhcpEnabled);
                        Console.WriteLine("DynamicDns enabled:" + ni.IsDynamicDnsEnabled);
                        Console.WriteLine("NetworkInterfaceType:" + ni.NetworkInterfaceType.ToString());
                        Console.WriteLine("PhysicalAddress:" + BytesToHexString( ni.PhysicalAddress ) );
                        foreach( string dnsa in ni.DnsAddresses)
                        {
                            Console.WriteLine("Dns address:" + dnsa);
                        }
                        if ( ni.GetType() == typeof(Wireless80211))
                        {
                            Console.WriteLine("Wireless");
                            Wireless80211 wi = ni as Wireless80211;

                            Console.WriteLine("Ssid:" + wi.Ssid.ToString());

                        }
                    }

                    loopCount++;
                    // Test code for DHCP and Static adapter configs
                    // Switch every 5 loops between the two
                    if (loopCount % 10 == 0)
                    {
                        Console.WriteLine("Switch DHCP");
                        // switch to dhcp address, get Esp32 Ethernet interface
                        NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[2];
                        ni.EnableDhcp();
 
                    }
                    else
                    if (loopCount % 5 == 0)
                    {
                        Console.WriteLine("Switch static IP");
                        // switch to static address, get Esp32 Ethernet interface
                        NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[2];
                        ni.EnableStaticIP("192.168.2.234", "255.255.255.0", "192.168.2.1");
                    }

                    //{
                    //    Wireless80211 wi = NetworkInterface.GetAllNetworkInterfaces()[0] as Wireless80211;
                    //    wi.Ssid = "MySsid";
                    //    wi.PassPhrase = "aPassword";

                    //    Wireless80211.ValidateConfiguration(wi);
                    //    Wireless80211.SaveConfiguration( new Wireless80211[] { wi }, true);

                    //}
                    Thread.Sleep(10000);
                }


            }
            catch (Exception ex)
            {
                // Do whatever please you with the exception caught
                Console.WriteLine("Exception:" + ex.Message);
                
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
            Console.WriteLine("NetworkChange_NetworkAddressChanged" );
            if (sender==null) Console.WriteLine("NetworkChange_NetworkAddressChanged sender null");
        }

        private static void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            Console.WriteLine("NetworkChange_NetworkAvailabilityChanged:"+e.IsAvailable.ToString());
            if (sender == null) Console.WriteLine("NetworkChange_NetworkAvailabilityChanged sender null");
        }
    }
}
