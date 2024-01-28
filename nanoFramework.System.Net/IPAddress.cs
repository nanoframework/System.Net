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
        internal const int IPv4AddressBytes = 4;
        internal const int IPv6AddressBytes = 16;

        internal const int NumberOfLabels = IPv6AddressBytes / 2;

        /// <summary>
        /// Provides an IP address that indicates that the server must listen for client activity on all network interfaces. This field is read-only.
        /// </summary>
        /// <remarks>
        /// The <see cref="Socket.Bind"/> method uses the <see cref="Any"/> field to indicate that a <see cref="Socket"/> instance must listen for client activity on all network interfaces.
        /// 
        /// The <see cref="Any"/> field is equivalent to 0.0.0.0 in dotted-quad notation.
        /// </remarks>
        public static readonly IPAddress Any = new(new byte[] { 0, 0, 0, 0 });

        /// <summary>
        /// Provides the IP loopback address. This field is read-only.
        /// </summary>
        /// <remarks>
        /// The <see cref="Loopback"/> field is equivalent to 127.0.0.1 in dotted-quad notation.
        /// </remarks>
        public static readonly IPAddress Loopback = new(new byte[] { 127, 0, 0, 1 });

        /// <summary>
        /// Provides the IP broadcast address. This field is read-only.
        /// </summary>
        /// <remarks>
        /// The <see cref="Broadcast"/> field is equivalent to 255.255.255.255 in dotted-quad notation.
        /// </remarks>
        public static readonly IPAddress Broadcast = new(new byte[] { 255, 255, 255, 255 });

        /// <summary>
        /// Provides an IP address that indicates that no network interface should be used. This field is read-only.
        /// </summary>
        /// <remarks>
        /// The <see cref="Socket.Bind"/> uses the <see cref="None"/> field to indicate that a <see cref="Socket"/> must not listen for client activity.
        ///
        /// The <see cref="None"/> field is equivalent to 255.255.255.255 in dotted-quad notation.
        /// </remarks>
        public static readonly IPAddress None = Broadcast;

        internal readonly long Address;

        /// <summary>
        /// The Bind(EndPoint) method uses the IPv6Any field to indicate that a Socket must listen for client activity on all network interfaces.
        /// </summary>
        public static readonly IPAddress IPv6Any = new IPAddress(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0);

        /// <summary>
        /// Provides the IP loopback address. This property is read-only.
        /// </summary>
        public static readonly IPAddress IPv6Loopback = new IPAddress(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }, 0);
        

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private readonly AddressFamily _family = AddressFamily.InterNetwork;

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private readonly ushort[] _numbers = new ushort[NumberOfLabels];

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private long _scopeid = 0;

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
        /// <param name="address">The byte array value of the IP address.</param>
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
        /// Indicates whether two <see cref="IPAddress"/> objects are equal.
        /// </summary>
        /// <param name="a">The <see cref="IPAddress"/> to compare with <paramref name="b"/>.</param>
        /// <param name="b">The <see cref="IPAddress"/> to compare with <paramref name="a"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="b"/> is equal to <paramref name="a"/>; otherwise, <see langword="false"/>.</returns>   
        public static bool operator ==(IPAddress a, IPAddress b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            return a is not null && a.Equals(b);
        }

        /// <summary>
        /// Indicates whether two <see cref="IPAddress"/> objects are not equal.
        /// </summary>
        /// <param name="a">The <see cref="IPAddress"/> to compare with <paramref name="b"/>.</param>
        /// <param name="b">The <see cref="IPAddress"/> to compare with <paramref name="a"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="b"/> is not equal to <paramref name="a"/>; otherwise, <see langword="false"/>.</returns>   
        public static bool operator !=(IPAddress a, IPAddress b) => !(a == b);

        /// <summary>
        /// Initializes a new instance of a IPV6 <see cref="IPAddress"/> class with the address specified as a Byte array.
        /// </summary>
        /// <param name="address">The byte array value of the IP address.</param>
        /// <param name="scopeid">The long value of the scope identifier.</param>
        /// <exception cref="ArgumentNullException"><paramref name="address"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="address"/> contains a bad IP address.</exception>
        /// <remarks>
        /// The IPAddress is created with the <see cref="Address"/> property set to <paramref name="address"/>.
        /// If the length of <paramref name="address"/> is 4, <see cref="IPAddress"/>(Byte[]) constructs an IPv4 address; otherwise, an IPv6 address with a scope of 0 is constructed.
        /// The <see cref="Byte"/> array is assumed to be in network byte order with the most significant byte first in index position 0.
        /// </remarks>
        public IPAddress(byte[] address, long scopeid) : this(address) 
        {
            if (address.Length == IPv6AddressBytes)
            {
                _scopeid = scopeid;
            }
            else
            {
                // Not IPV6 address
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentException();
#pragma warning restore S3928 // OK to throw this here
            }
        }

        private IPAddress(ushort[] address, uint scopeid)
        {
            _family = AddressFamily.InterNetworkV6;
            _numbers = address;
            _scopeid = scopeid;
        }

        /// <summary>
        /// Compares two IP addresses.
        /// </summary>
        /// <param name="other">An <see cref="IPAddress"/> instance to compare to the current instance.</param>
        /// <returns><see langword="true"/> if the two addresses are equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object other)
        {
            return other is IPAddress ipAddress && Equals(ipAddress);
        }

        /// <summary>
        /// Compares two IP addresses.
        /// </summary>
        /// <param name="other">An <see cref="IPAddress"/> instance to compare to the current instance.</param>
        /// <returns><see langword="true"/> if the two addresses are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(IPAddress other)
        {
            if (other is null)
            {
                return false;
            }

            // Compare families before address representations
            if (AddressFamily != other.AddressFamily)
            {
                return false;
            }

            // Compare family before address
            if (_family != other.AddressFamily)
            {
                return false;
            }

            if (_family == AddressFamily.InterNetworkV6)
            {
                // For IPv6 addresses, compare the full 128bit address
                for (var i = 0; i < NumberOfLabels; i++)
                {
                    if (other._numbers[i] != _numbers[i])
                        return false;
                }

                // Also scope must match
                return other._scopeid == _scopeid;
            }
            else
            {
                return Address == other.Address;
            }
        }

        /// <summary>
        /// Provides a copy of the <see cref="IPAddress"/> as an array of bytes in network order.
        /// </summary>
        /// <returns>A <see langword="byte"/> array.</returns>
        public byte[] GetAddressBytes()
        {
            byte[] bytes;

            if (_family == AddressFamily.InterNetworkV6)
            {
                bytes = new byte[NumberOfLabels * 2];

                int j = 0;
                for (int i = 0; i < NumberOfLabels; i++)
                {
                    bytes[j++] = (byte)((_numbers[i] >> 8) & 0xFF);
                    bytes[j++] = (byte)((_numbers[i]) & 0xFF);
                }
                return bytes;
            }
            else
            {
                return new byte[]
                {
                    (byte)(Address),
                    (byte)(Address >> 8),
                    (byte)(Address >> 16),
                    (byte)(Address >> 24)
                };
            }
        }

        /// <summary>
        /// Converts an IP address string to an <see cref="IPAddress"/> instance.
        /// </summary>
        /// <param name="ipString">A string that contains an IP address in dotted-quad notation for IPv4 and in colon-hexadecimal notation for IPv6.</param>
        /// <returns>An <see cref="IPAddress"/> instance.
        /// </returns>
        public static IPAddress Parse(string ipString)
        {
            // Check for IPV6 string and use separate parse method 
            if (ipString.IndexOf(':') != -1)
            {
                return new IPAddress(NetworkInterface.IPV6AddressFromString(ipString), 0);
            }

            return new IPAddress(NetworkInterface.IPV4AddressFromString(ipString));
        }

        /// <summary>
        /// Get or Set IPV6 scope identifier.
        /// </summary>
        public long ScopeId
        {
            get 
            {
                // Not valid for IPv4 addresses
                if (_family == AddressFamily.InterNetwork)
                {
                    throw new SocketException(SocketError.OperationNotSupported);
                }

                return _scopeid; 
            }
            set 
            {
                // Not valid for IPv4 addresses
                if (_family == AddressFamily.InterNetwork)
                {
                    throw new SocketException(SocketError.OperationNotSupported);
                }

                _scopeid = value; 
            }
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
            if (_family == AddressFamily.InterNetworkV6)
            {
                return IPv6ToString(_numbers);
            }

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

            // For IPv4 addresses, we can simply use the integer representation.
            return unchecked((int)Address);
        }

        // For security, we need to be able to take an IPAddress and make a copy that's immutable and not derived.
        internal IPAddress Snapshot()
        {
            switch (_family)
            {
                case AddressFamily.InterNetwork:
                    return new IPAddress(Address);

                case AddressFamily.InterNetworkV6:
                    return new IPAddress(_numbers, (uint)_scopeid);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets whether the IP address is an IPv4-mapped IPv6 address.
        /// </summary>
        /// <returns>
        /// true if the IP address is an IPv4-mapped IPv6 address; otherwise, false.
        /// </returns>
        public bool IsIPv4MappedToIPv6
        {
            get
            {
                if (AddressFamily != AddressFamily.InterNetworkV6)
                {
                    return false;
                }

                for (int i = 0; i < 5; i++)
                {
                    if (_numbers[i] != 0)
                    {
                        return false;
                    }
                }

                return (_numbers[5] == 0xFFFF);
            }
        }

        /// <summary>
        /// Maps the <see cref="IPAddress"/> object to an IPv6 address.
        /// </summary>
        /// <remarks>
        /// Dual-stack sockets always require IPv6 addresses. The ability to interact with an IPv4 address 
        /// requires the use of the IPv4-mapped IPv6 address format. Any IPv4 addresses must be represented 
        /// in the IPv4-mapped IPv6 address format which enables an IPv6 only application to communicate 
        /// with an IPv4 node.
        /// For example.  IPv4 192.168.1.4 maps as IPV6 ::FFFF:192.168.1.4
        /// </remarks>
        /// <returns>
        /// Returns <see cref="IPAddress"/>. An IPV6 address.
        /// </returns>
        public IPAddress MapToIPv6()
        {
            if (AddressFamily == AddressFamily.InterNetworkV6)
            {
                return this;
            }

            ushort[] labels = new ushort[NumberOfLabels];
            labels[5] = 0xFFFF;
            labels[6] = (ushort)(((Address & 0x0000FF00) >> 8) | ((Address & 0x000000FF) << 8));
            labels[7] = (ushort)(((Address & 0xFF000000) >> 24) | ((Address & 0x00FF0000) >> 8));

            return new IPAddress(labels, 0);
        }

        /// <summary>
        /// Maps the <see cref="IPAddress"/> object to an IPv4 address.
        /// </summary>
        /// <remarks> 
        /// Dual-stack sockets always require IPv6 addresses. The ability to interact with an IPv4 
        /// address requires the use of the IPv4-mapped IPv6 address format. 
        /// </remarks>
        /// <returns>
        /// Returns <see cref="IPAddress"/>. An IPV4 address.
        /// </returns>
        public IPAddress MapToIPv4()
        {
            if (AddressFamily == AddressFamily.InterNetwork)
            {
                return this;
            }

            long address =  ((((uint)_numbers[6] & 0x0000FF00u) >> 8) | 
                            (((uint)_numbers[6] & 0x000000FFu) << 8)) |
                            (((((uint)_numbers[7] & 0x0000FF00u) >> 8) | 
                            (((uint)_numbers[7] & 0x000000FFu) << 8)) << 16);

            return new IPAddress(address);
        }

        #region native methods

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern string IPv4ToString(uint ipv4Address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern string IPv6ToString(ushort[] ipv6Address);
        #endregion
    }
}
