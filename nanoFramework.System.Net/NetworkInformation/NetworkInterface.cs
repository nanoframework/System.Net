//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Net.NetworkInformation
{
    /// <summary>
    /// Provides information about network interfaces and enables applications to control them.
    /// </summary>
    /// <remarks>
    /// This class is exclusive of nanoFramework and it does not exist on the UWP API.
    /// </remarks>
    public class NetworkInterface
    {
        //set update flags...
        [Flags]
        internal enum UpdateOperation : byte
        {
            Invalid =       0x00,
            Dns =           0x01,
            Dhcp =          0x02,
            DhcpRenew =     0x04,
            DhcpRelease =   0x08,
            Mac =           0x10,
        }

        private readonly int _interfaceIndex;

        private byte[] _macAddress;
        private AddressMode _startupAddressMode;
        private uint _specificConfigId;
        private bool _automaticDns;

        // IPv4 fields
        private uint _ipv4Address;
        private uint _ipv4NetMask;
        private uint _ipv4GatewayAddress;
        private uint _ipv4dnsAddress1;
        private uint _ipv4dnsAddress2;

        // IPv6 fields
        private uint[] _ipv6Address;
        private uint[] _ipv6NetMask;
        private uint[] _ipv6GatewayAddress;
        private uint[] _ipv6dnsAddress1;
        private uint[] _ipv6dnsAddress2;

        private NetworkInterfaceType _networkInterfaceType;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkInterface"/> class.
        /// </summary>
        /// <param name="interfaceIndex"></param>
        protected NetworkInterface(int interfaceIndex)
        {
            _interfaceIndex = interfaceIndex;
            _networkInterfaceType = NetworkInterfaceType.Unknown;
            _startupAddressMode = AddressMode.Invalid;
            _specificConfigId = uint.MaxValue;
            _automaticDns = true;

            _ipv6Address = new uint[4];
            _ipv6NetMask = new uint[4];
            _ipv6GatewayAddress = new uint[4];
            _ipv6dnsAddress1 = new uint[4];
            _ipv6dnsAddress2 = new uint[4];
        }

        /// <summary>
        /// Retrieves an array of all of the device's network interfaces.
        /// </summary>
        /// <returns>An array containing all of the device's network interfaces. </returns>
        public static NetworkInterface[] GetAllNetworkInterfaces()
        {
            int count = GetNetworkInterfaceCount();
            NetworkInterface[] ifaces = new NetworkInterface[count];

            for (uint i = 0; i < count; i++)
            {
                ifaces[i] = GetNetworkInterface(i);
            }

            return ifaces;
        }

        private string IPv4AddressToString(uint ipv4Address)
        {
            return string.Concat(
                            ((ipv4Address >> 0) & 0xFF).ToString(),
                                ".",
                            ((ipv4Address >> 8) & 0xFF).ToString(),
                                ".",
                            ((ipv4Address >> 16) & 0xFF).ToString(),
                                ".",
                            ((ipv4Address >> 24) & 0xFF).ToString()
                            );
        }

        private string IPv6AddressToString(uint[] ipv6Address)
        {
            throw new NotImplementedException();

        //    return string.Concat(
        //                    ipv6Address[0].ToString("X4"),
        //                        ":",
        //                    ipv6Address[1].ToString("X4"),
        //                        ".",
        //                    ipv6Address[2].ToString("X4"),
        //                        ".",
        //                    ipv6Address[3].ToString("X4")
        //                    );
        }

        /// <summary>
        /// Enables an application to set and use a static IPv4 address.
        /// </summary>
        /// <param name="ipv4Address">Holds the IPv4 address to use. </param>
        /// <param name="ipv4SubnetMask">Contains the IPv4 address's subnet mask.</param>
        /// <param name="ipv4GatewayAddress">Specifies the IPv4 address of the gateway. </param>
        public void EnableStaticIPv4(string ipv4Address, string ipv4SubnetMask, string ipv4GatewayAddress)
        {
            try
            {
                _ipv4Address = IPAddressFromString(ipv4Address);
                _ipv4NetMask = IPAddressFromString(ipv4SubnetMask);
                _ipv4GatewayAddress = IPAddressFromString(ipv4GatewayAddress);
                _startupAddressMode = AddressMode.Static;

                UpdateConfiguration((int)UpdateOperation.Dhcp);
            }
            finally
            {
                ReloadSettings();
            }
        }

        /// <summary>
        /// Enables an application to set and use a static IPv6 address.
        /// </summary>
        /// <param name="ipv6Address">Holds the IPv6 address to use. </param>
        /// <param name="ipv6SubnetMask">Contains the IPv6 address's subnet mask.</param>
        /// <param name="ipv6GatewayAddress">Specifies the IPv6 address of the gateway. </param>
        public void EnableStaticIPv6(string ipv6Address, string ipv6SubnetMask, string ipv6GatewayAddress)
        {
            try
            {
                throw new NotImplementedException();

                // FIXME
                // need to test this
                //_ipv6Address = IPAddressFromString(ipv6Address);
                //_ipv6NetMask = IPAddressFromString(ipv6subnetMask);
                //_ipv6GatewayAddress = IPAddressFromString(ipv6gatewayAddress);

                //_startupAddressMode = AddressMode.Static;

                //UpdateConfiguration(UPDATE_FLAGS_DHCP);
            }
            finally
            {
                ReloadSettings();
            }
        }

        /// <summary>
        /// Enables an application to set and use a static IPv4 and IPv6 address.
        /// </summary>
        /// <param name="ipv4Address">Holds the IPv4 address to use. </param>
        /// <param name="ipv4subnetMask">Contains the IPv4 address's subnet mask.</param>
        /// <param name="ipv4gatewayAddress">Specifies the IPv4 address of the gateway. </param>
        /// <param name="ipv6Address">Holds the IPv6 address to use. </param>
        /// <param name="ipv6SubnetMask">Contains the IPv6 address's subnet mask.</param>
        /// <param name="ipv6GatewayAddress">Specifies the IPv6 address of the gateway. </param>
        public void EnableStaticIP(string ipv4Address, string ipv4subnetMask, string ipv4gatewayAddress, string ipv6Address, string ipv6SubnetMask, string ipv6GatewayAddress)
        {
            try
            {
                throw new NotImplementedException();

                _ipv4Address = IPAddressFromString(ipv4Address);
                _ipv4NetMask = IPAddressFromString(ipv4subnetMask);
                _ipv4GatewayAddress = IPAddressFromString(ipv4gatewayAddress);

                // FIXME
                // need to test this
                //_ipv6Address = IPAddressFromString(ipv6Address);
                //_ipv6NetMask = IPAddressFromString(ipv6subnetMask);
                //_ipv6GatewayAddress = IPAddressFromString(ipv6gatewayAddress);

                _startupAddressMode = AddressMode.Static;

                UpdateConfiguration((int)UpdateOperation.Dhcp);
            }
            finally
            {
                ReloadSettings();
            }
        }

        /// <summary>
        /// Enables the Dynamic Host Configuration Protocol (DHCP) for service with this network interface.
        /// </summary>
        public void EnableDhcp()
        {
            try
            {
                _startupAddressMode = AddressMode.DHCP;
                UpdateConfiguration((int)UpdateOperation.Dhcp);
            }
            finally
            {
                ReloadSettings();
            }
        }

        /// <summary>
        /// Enables a network interface to use a specific DNS server IPv4 address.
        /// </summary>
        /// <param name="dnsAddresses">Holds the DNS server address. </param>
        public void EnableStaticIPv4Dns(string[] dnsAddresses)
        {
            if (dnsAddresses == null || dnsAddresses.Length == 0 || dnsAddresses.Length > 2)
            {
                throw new ArgumentException();
            }

            uint[] addresses = new uint[2];

            int iAddress = 0;
            for (int i = 0; i < dnsAddresses.Length; i++)
            {
                uint address = IPAddressFromString(dnsAddresses[i]);

                addresses[iAddress] = address;

                if (address != 0)
                {
                    iAddress++;
                }
            }

            try
            {
                _ipv4dnsAddress1 = addresses[0];
                _ipv4dnsAddress2 = addresses[1];

                // clear flag
                _automaticDns = false;

                UpdateConfiguration((int)UpdateOperation.Dns);
            }
            finally
            {
                ReloadSettings();
            }
        }

        /// <summary>
        /// Enables a network interface to use a specific DNS server IPv6 address.
        /// </summary>
        /// <param name="dnsAddresses">Holds the DNS server address. </param>
        public void EnableStaticIPv6Dns(string[] dnsAddresses)
        {
            throw new NotImplementedException();

            // reset flag
            //_flags &= ~Options.DynamicDNS;

            //if (dnsAddresses == null || dnsAddresses.Length == 0 || dnsAddresses.Length > 2)
            //{
            //    throw new ArgumentException();
            //}

            //uint[] addresses = new uint[2];

            //int iAddress = 0;
            //for (int i = 0; i < dnsAddresses.Length; i++)
            //{
            //    uint address = IPAddressFromString(dnsAddresses[i]);

            //    addresses[iAddress] = address;

            //    if (address != 0)
            //    {
            //        iAddress++;
            //    }
            //}

            //try
            //{
            //    _ipv6dnsAddress1 = addresses[0];
            //    _ipv6dnsAddress2 = addresses[1];

            //    UpdateConfiguration(UPDATE_FLAGS_DNS);
            //}
            //finally
            //{
            //    ReloadSettings();
            //}
        }

        /// <summary>
        /// Enables a network interface to obtain a DNS server address automatically.
        /// </summary>
        public void EnableAutomaticDns()
        {
            try
            {
                // reset IPv4 DNS addresses
                _ipv4dnsAddress1 = 0;
                _ipv4dnsAddress2 = 0;

                // set flag
                _automaticDns = true;

                UpdateConfiguration((int)UpdateOperation.Dns);
            }
            finally
            {
                ReloadSettings();
            }
        }

        /// <summary>
        /// Retrieves a value indicating whether a network interface can obtain a DNS server address automatically.
        /// true if dynamic DNS is enabled, or false if not. 
        /// </summary>
        public bool IsAutomaticDnsEnabled
        {
            get
            {
                return _automaticDns;
            }
        }

        /// <summary>
        /// Holds the IP v4 address of the network interface.
        /// </summary>
        public string IPv4Address
        {
            get { return IPv4AddressToString(_ipv4Address); }
        }

        /// <summary>
        /// Contains the gateway IPv4 address.
        /// </summary>
        public string IPv4GatewayAddress
        {
            get { return IPv4AddressToString(_ipv4GatewayAddress); }
        }

        /// <summary>
        /// Retrieves the network interface's IPv4 subnet mask.
        /// </summary>
        public string IPv4SubnetMask
        {
            get { return IPv4AddressToString(_ipv4NetMask); }
        }

        /// <summary>
        /// Gets a value specifying whether DHCP is enabled for this network interfaces.
        /// true if DHCP is enabled, or false if not. 
        /// </summary>
        public bool IsDhcpEnabled
        {
            get { return (_startupAddressMode == AddressMode.DHCP);  }
        }

        /// <summary>
        /// Holds the IPv4 DNS server address.
        /// </summary>
        public string[] IPv4DnsAddresses
        {
            get
            {
                ArrayList list = new ArrayList();

                if (_ipv4dnsAddress1 != 0)
                {
                    list.Add(IPv4AddressToString(_ipv4dnsAddress1));
                }

                if (_ipv4dnsAddress2 != 0)
                {
                    list.Add(IPv4AddressToString(_ipv4dnsAddress2));
                }

                return (string[])list.ToArray(typeof(string));
            }
        }

        private void ReloadSettings()
        {
            Thread.Sleep(100);
            InitializeNetworkInterfaceSettings();
        }

        /// <summary>
        /// Releases the DHCP lease, which releases the IP address bound to a DHCP-enabled network interface.
        /// </summary>
        public void ReleaseDhcpLease()
        {
            try
            {
                UpdateConfiguration((int)UpdateOperation.DhcpRelease);
            }
            finally
            {
                ReloadSettings();
            }
        }

        /// <summary>
        /// Renews a DHCP lease, which renews the IP address on a DHCP-enabled network interface.
        /// </summary>
        public void RenewDhcpLease()
        {
            try
            {
                UpdateConfiguration((int)UpdateOperation.DhcpRenew);
            }
            finally
            {
                ReloadSettings();
            }
        }

        /// <summary>
        /// Gets or sets the Media Access Control (MAC) address for a network interface.
        /// </summary>
        public byte[] PhysicalAddress
        {
            get { return _macAddress; }
            set
            {
                try
                {
                    _macAddress = value;
                    UpdateConfiguration((int)UpdateOperation.Mac);
                }
                finally
                {
                    ReloadSettings();
                }
            }
        }

        /// <summary>
        /// Retrieves a value specifying the type of network interface being used by the device.
        /// </summary>
        public NetworkInterfaceType NetworkInterfaceType
        {
            get { return _networkInterfaceType; }
        }

        /// <summary>
        /// The ID of the associated configuration, if any. To be used as the foreign key of that configuration.
        /// </summary>
        /// <remarks>
        /// If there is no configuration associated it reads as <see cref="uint.MaxValue"/>.
        /// </remarks>
        public uint SpecificConfigId { get => _specificConfigId; set => _specificConfigId = value; }

        #region native methods

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static int GetNetworkInterfaceCount();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static NetworkInterface GetNetworkInterface(uint interfaceIndex);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void InitializeNetworkInterfaceSettings();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void UpdateConfiguration(int updateType);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern uint IPAddressFromString(string ipAddress);

        #endregion
    }
}


