////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Net.NetworkInformation
{
    /// <summary>
    /// Specifies the type of network interface used by the device.
    /// </summary>
    public enum NetworkInterfaceType
    {
        /// <summary>
        /// The network interface type is unknown or not specified.
        /// </summary>
        Unknown = 1,
        /// <summary>
        /// The device uses an Ethernet network interface.
        /// </summary>
        Ethernet = 6,
        /// <summary>
        /// The device uses a wireless network based on the 802.11 standard.
        /// </summary>
        Wireless80211 = 71,
    }

    /// <summary>
    /// Provides information about interfaces and enables applications to control them.
    /// </summary>
    public class NetworkInterface
    {
        //set update flags...
        private const int UPDATE_FLAGS_DNS = 0x1;
        private const int UPDATE_FLAGS_DHCP = 0x2;
        private const int UPDATE_FLAGS_DHCP_RENEW = 0x4;
        private const int UPDATE_FLAGS_DHCP_RELEASE = 0x8;
        private const int UPDATE_FLAGS_MAC = 0x10;

        private const uint FLAGS_DHCP = 0x1;
        private const uint FLAGS_DYNAMIC_DNS = 0x2;

 //FIXME       [FieldNoReflection]
        private readonly int _interfaceIndex;

        private uint _flags;
        private uint _ipAddress;
        private uint _gatewayAddress;
        private uint _subnetMask;
        private uint _dnsAddress1;
        private uint _dnsAddress2;
        private NetworkInterfaceType _networkInterfaceType;
        private byte[] _macAddress;

        /// <summary>
        ///  	Initializes a new instance of the NetworkInterface class.
        /// </summary>
        /// <param name="interfaceIndex"></param>
        protected NetworkInterface(int interfaceIndex)
        {
            this._interfaceIndex = interfaceIndex;
            _networkInterfaceType = NetworkInterfaceType.Unknown;
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

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static int GetNetworkInterfaceCount();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static NetworkInterface GetNetworkInterface(uint interfaceIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void InitializeNetworkInterfaceSettings();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void UpdateConfiguration(int updateType);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern uint IPAddressFromString(string ipAddress);

        private string IPAddressToString(uint ipAddress)
        {
 // FIXME          if(SystemInfo.IsBigEndian)
            //{
            //    return string.Concat(
            //                    ((ipAddress >> 24) & 0xFF).ToString(),
            //                     ".",
            //                    ((ipAddress >> 16) & 0xFF).ToString(),
            //                     ".",
            //                    ((ipAddress >> 8) & 0xFF).ToString(),
            //                     ".",
            //                    ((ipAddress >> 0) & 0xFF).ToString()
            //                    );
            //}
            //    else
            {
                return string.Concat(
                                ((ipAddress >> 0) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 8) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 16) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 24) & 0xFF).ToString()
                                );
            }
        }

        /// <summary>
        /// Enables an application to set and use a static IP address.
        /// </summary>
        /// <param name="ipAddress">Holds the IP address to use. </param>
        /// <param name="subnetMask">Contains the address's subnet mask.</param>
        /// <param name="gatewayAddress">Specifies the address of the gateway. </param>
        public void EnableStaticIP(string ipAddress, string subnetMask, string gatewayAddress)
        {
            try
            {
                _ipAddress = IPAddressFromString(ipAddress);
                _subnetMask = IPAddressFromString(subnetMask);
                _gatewayAddress = IPAddressFromString(gatewayAddress);
                _flags &= ~FLAGS_DHCP;

                UpdateConfiguration(UPDATE_FLAGS_DHCP);
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
                _flags |= FLAGS_DHCP;
                UpdateConfiguration(UPDATE_FLAGS_DHCP);
            }
            finally
            {
                ReloadSettings();
            }
        }

        /// <summary>
        /// Enables a network interface to use a specific DNS server address.
        /// </summary>
        /// <param name="dnsAddresses">Holds the DNS server address. </param>
        public void EnableStaticDns(string[] dnsAddresses)
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
                _dnsAddress1 = addresses[0];
                _dnsAddress2 = addresses[1];

                _flags &= ~FLAGS_DYNAMIC_DNS;

                UpdateConfiguration(UPDATE_FLAGS_DNS);
            }
            finally
            {
                ReloadSettings();
            }
        }

        /// <summary>
        /// Enables a network interface to obtain a DNS server address automatically.
        /// </summary>
        public void EnableDynamicDns()
        {
            try
            {
                _flags |= FLAGS_DYNAMIC_DNS;

                UpdateConfiguration(UPDATE_FLAGS_DNS);
            }
            finally
            {
                ReloadSettings();
            }
        }

        /// <summary>
        /// Holds the IP address of the network interface.
        /// </summary>
        public string IPAddress
        {
            get { return IPAddressToString(_ipAddress); }
        }

        /// <summary>
        /// Contains the gateway address.
        /// </summary>
        public string GatewayAddress
        {
            get { return IPAddressToString(_gatewayAddress); }
        }

        /// <summary>
        /// Retrieves the network interface's subnet mask.
        /// </summary>
        public string SubnetMask
        {
            get { return IPAddressToString(_subnetMask); }
        }

        /// <summary>
        /// Gets a value specifying whether DHCP is enabled for this network interfaces.
        /// true if DHCP is enabled, or false if not. 
        /// </summary>
        public bool IsDhcpEnabled
        {
            get { return (_flags & FLAGS_DHCP) != 0; }
        }

        /// <summary>
        /// Retrieves a value indicating whether a network interface can obtain a DNS server address automatically.
        /// true if dynamic DNS is enabled, or false if not. 
        /// </summary>
        public bool IsDynamicDnsEnabled
        {
            get
            {
                return (_flags & FLAGS_DYNAMIC_DNS) != 0;
            }
        }

        /// <summary>
        /// Holds the DNS server address.
        /// </summary>
        public string[] DnsAddresses
        {
            get
            {
                ArrayList list = new ArrayList();

                if (_dnsAddress1 != 0)
                {
                    list.Add(IPAddressToString(_dnsAddress1));
                }

                if (_dnsAddress2 != 0)
                {
                    list.Add(IPAddressToString(_dnsAddress2));
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
                UpdateConfiguration(UPDATE_FLAGS_DHCP_RELEASE);
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
                UpdateConfiguration(UPDATE_FLAGS_DHCP_RELEASE | UPDATE_FLAGS_DHCP_RENEW);
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
                    UpdateConfiguration(UPDATE_FLAGS_MAC);
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
    }
}


