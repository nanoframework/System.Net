//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace System.Net
{
    /// <summary>
    /// Provides an internet protocol (IP) address.
    /// </summary>
    [Serializable]
    public class IPAddress
    {
        /// <summary>
        /// Provides an IP address that indicates that the server must listen for client activity on all network interfaces. 
        /// This field is read-only.
        /// </summary>
        public static readonly IPAddress Any = new IPAddress(0x0000000000000000);

        /// <summary>
        /// Provides the IP loopback address. This field is read-only.
        /// </summary>
        public static readonly IPAddress Loopback = new IPAddress(0x000000000100007F);

        internal long Address;

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private AddressFamily _family = AddressFamily.InterNetwork;

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private ushort[] _numbers = new ushort[NumberOfLabels];

        internal const int IPv4AddressBytes = 4;
        internal const int IPv6AddressBytes = 16;

        internal const int NumberOfLabels = IPv6AddressBytes / 2;

        /// <summary>
        /// Gets the address family of the IP address.
        /// </summary>
        /// <value>Returns <see cref="AddressFamily.InterNetwork"/> for IPv4 or <see cref="AddressFamily.InterNetworkV6"/> for IPv6.</value>
        public AddressFamily AddressFamily
        {
            get
            {
                return _family;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddress"/> class with the address specified as an Int64.
        /// </summary>
        /// <param name="newAddress"></param>
        public IPAddress(long newAddress)
        {
            if (newAddress < 0 || newAddress > 0x00000000FFFFFFFF)
            {
                throw new ArgumentOutOfRangeException();
            }

            Address = newAddress;

            // default to InterNetwork
            _family = AddressFamily.InterNetwork;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddress"/> class with the address specified as a Byte array.
        /// </summary>
        /// <param name="address"></param>
        public IPAddress(byte[] address)
        {
            if (address == null)
            {
                throw new ArgumentNullException();
            }

            if (address.Length != IPv4AddressBytes && address.Length != IPv6AddressBytes)
            {
                // unsupported address family
                throw new NotSupportedException();
            }

            if (address.Length == IPv4AddressBytes)
            {
                _family = AddressFamily.InterNetwork;
                Address = ((address[3] << 24 | address[2] << 16 | address[1] << 8 | address[0]) & 0x0FFFFFFFF);
            }
            else
            {
                _family = AddressFamily.InterNetworkV6;

                for (int i = 0; i < NumberOfLabels; i++)
                {
                    _numbers[i] = (ushort)(address[i * 2] * 256 + address[i * 2 + 1]);
                }
            }
        }

        /// <summary>
        /// Compares two IP addresses.
        /// </summary>
        /// <param name="obj">An <see cref="IPAddress"/> instance to compare to the current instance.</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            IPAddress addr = obj as IPAddress;

            if (obj == null) return false;

            return this.Address == addr.Address;
        }

        /// <summary>
        /// Provides a copy of the <see cref="IPAddress"/> as an array of bytes.
        /// </summary>
        /// <returns>A Byte array.</returns>
        public byte[] GetAddressBytes()
        {
            return new byte[]
            {
                (byte)(Address),
                (byte)(Address >> 8),
                (byte)(Address >> 16),
                (byte)(Address >> 24)
            };
        }

        /// <summary>
        /// Converts an IP address string to an <see cref="IPAddress"/> instance.
        /// </summary>
        /// <param name="ipString">A string that contains an IP address in dotted-quad notation for IPv4 and in colon-hexadecimal notation for IPv6.</param>
        /// <returns>An <see cref="IPAddress"/> instance.
        /// </returns>
        public static IPAddress Parse(string ipString)
        {
            return new IPAddress(NetworkInterface.IPAddressFromString(ipString));
        }

        /// <summary>
        /// Converts an Internet address to its standard notation.
        /// </summary>
        /// <returns>A string that contains the IP address in either IPv4 dotted-quad or 
        /// in IPv6 colon-hexadecimal notation.
        /// </returns>
        /// <remarks>
        /// The ToString method converts the IP address that is stored in the Address property to either IPv4 dotted-quad or 
        /// IPv6 colon-hexadecimal notation.
        /// </remarks>
        public override string ToString()
        {
            return IPv4ToString((uint)Address);
        }

        /// <summary>
        /// Retrieves an IP address that is the local default address.
        /// </summary>
        /// <returns>The default IP address.</returns>
        public static IPAddress GetDefaultLocalAddress()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            int cnt = interfaces.Length;
            for (int i = 0; i < cnt; i++)
            {
                NetworkInterface ni = interfaces[i];

                if (ni.IPv4Address != "0.0.0.0" && ni.IPv4SubnetMask != "0.0.0.0")
                {
                    return Parse(ni.IPv4Address);
                }
                // FIXME
                // TODO implement this when IPv6 support is added
                //else (ni.IPv6Address != "0.0.0.0" )
                //{
                //    return IPAddress.Parse(ni.IPv6Address);
                //}
            }

            return Any;
        }

        // For security, we need to be able to take an IPAddress and make a copy that's immutable and not derived.
        internal IPAddress Snapshot()
        {
            switch (_family)
            {
                case AddressFamily.InterNetwork:
                    return new IPAddress(Address);

                    //case AddressFamily.InterNetworkV6:
                    //    return new IPAddress(m_Numbers, (uint)m_ScopeId);
            }

            throw new NotSupportedException();
        }

        #region native methods

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern string IPv4ToString(uint ipv4Address);

        #endregion
    }
}
