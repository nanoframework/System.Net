// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.Networking
{
    /// <summary>
    /// IP configuration to be used for static IP address configuration.
    /// </summary>
    public class IPConfiguration
    {
        /// <summary>
        /// Constructor for IP Configuration.
        /// </summary>
        /// <param name="ipv4Address">The IP v4 address to use on the network interface.</param>
        /// <param name="ipv4SubnetMask">The IPv4 subnet mask.</param>
        /// <param name="ipv4GatewayAddress">The gateway IPv4 address.</param>
        /// <param name="ipv4DnsAddresses">List with the IPv4 DNS server address. Set to <see langword="null"/> for automatic DNS.</param>
        public IPConfiguration(
            string ipv4Address,
            string ipv4SubnetMask,
            string ipv4GatewayAddress,
            string[] ipv4DnsAddresses = null)
        {
            IPAddress = ipv4Address;
            IPSubnetMask = ipv4SubnetMask;
            IPGatewayAddress = ipv4GatewayAddress;
            IPDns = ipv4DnsAddresses;
        }

        /// <summary>
        /// IP v4 address to use on the network interface.
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// The IPv4 subnet mask.
        /// </summary>
        public string IPSubnetMask { get; set; }

        /// <summary>
        /// The gateway IPv4 address.
        /// </summary>
        public string IPGatewayAddress { get; set; }

        /// <summary>
        /// IPv4 DNS server address. Set to <see langword="null"/> for automatic DNS.
        /// </summary>
        public string[] IPDns { get; set; }
    }
}
