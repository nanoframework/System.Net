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
    /// Initializes a new instance of the <see cref="IPAddress"/> class.
    /// </summary>
    [Serializable]
    public class IPAddress
    {
        /// <summary>
        /// Provides an IP address that indicates that the server must listen for client activity on all network interfaces. This field is read-only.
        /// </summary>
        public static readonly IPAddress Any = new(0x0000000000000000);

        /// <summary>
        /// Provides the IP loopback address. This field is read-only.
        /// </summary>
        public static readonly IPAddress Loopback = new(0x000000000100007F);

        internal readonly long Address;

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private readonly AddressFamily _family = AddressFamily.InterNetwork;

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private readonly ushort[] _numbers = new ushort[NumberOfLabels];

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
        /// <param name="newAddress">The long value of the IP address. For example, the value 0x2414188f in big-endian format would be the IP address "143.24.20.36".</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="newAddress"/> &lt; 0 or
        /// <paramref name="newAddress"/> &gt; 0x00000000FFFFFFFF
        /// </exception>
        /// <remarks>
        /// The IPAddress instance is created with the <see cref="Address"/> property set to <paramref name="newAddress"/>.
        /// The <see cref="Int64"/> value is assumed to be in network byte order.
        /// </remarks>
        public IPAddress(long newAddress)
        {
            if (newAddress < 0 || newAddress > 0x00000000FFFFFFFF)
            {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentOutOfRangeException();
#pragma warning restore S3928 // OK to throw this here
            }

            Address = newAddress;

            // default to InterNetwork
            _family = AddressFamily.InterNetwork;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddress"/> class with the address specified as a Byte array.
        /// </summary>
        /// <param name="address"></param>
        /// <exception cref="ArgumentNullException"><paramref name="address"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="address"/> contains a bad IP address.</exception>
        /// <remarks>
        /// The IPAddress is created with the <see cref="Address"/> property set to <paramref name="address"/>.
        /// If the length of <paramref name="address"/> is 4, <see cref="IPAddress"/>(Byte[]) constructs an IPv4 address; otherwise, an IPv6 address with a scope of 0 is constructed.
        /// The <see cref="Byte"/> array is assumed to be in network byte order with the most significant byte first in index position 0.
        /// </remarks>
        public IPAddress(byte[] address)
        {
            if (address == null)
            {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentNullException();
#pragma warning restore S3928 // OK to throw this here
            }

            if (address.Length == IPv4AddressBytes)
            {
                _family = AddressFamily.InterNetwork;

                Address = (address[3] << 24 | address[2] << 16 | address[1] << 8 | address[0]) & 0x0FFFFFFFF;
            }
            else if (address.Length == IPv6AddressBytes)
            {
                _family = AddressFamily.InterNetworkV6;

                for (int i = 0; i < NumberOfLabels; i++)
                {
                    _numbers[i] = (ushort)(address[i * 2] * 256 + address[i * 2 + 1]);
                }
            }
            else
            {
                // unsupported address family
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentException();
#pragma warning restore S3928 // OK to throw this here
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
        /// <returns>A string that contains the IP address in either IPv4 dotted-quad or in IPv6 colon-hexadecimal notation.
        /// </returns>
        /// <remarks>
        /// The <see cref="ToString"/> method converts the IP address that is stored in the <see cref="Address"/> property to either IPv4 dotted-quad or IPv6 colon-hexadecimal notation.
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

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // For IPv6 addresses, we cannot simply return the integer
            // representation as the hashcode. Instead, we calculate
            // the hashcode from the string representation of the address.
            if (_family == AddressFamily.InterNetworkV6)
            {
                return ToString().GetHashCode();
            }
            else
            {
                // For IPv4 addresses, we can simply use the integer representation.
                return unchecked((int)Address);
            }
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
