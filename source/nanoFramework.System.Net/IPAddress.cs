//
// Copyright (c) 2018 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Net.NetworkInformation;

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
        internal long m_Address;

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

            m_Address = newAddress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddress"/> class with the address specified as a Byte array.
        /// </summary>
        /// <param name="newAddressBytes"></param>
        public IPAddress(byte[] newAddressBytes)
            : this(((((newAddressBytes[3] << 0x18) | (newAddressBytes[2] << 0x10)) | (newAddressBytes[1] << 0x08)) | newAddressBytes[0]) & ((long)0xFFFFFFFF))
        {
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

            return this.m_Address == addr.m_Address;
        }

        /// <summary>
        /// Provides a copy of the <see cref="IPAddress"/> as an array of bytes.
        /// </summary>
        /// <returns>A Byte array.</returns>
        public byte[] GetAddressBytes()
        {
            return new byte[]
            {
                (byte)(m_Address),
                (byte)(m_Address >> 8),
                (byte)(m_Address >> 16),
                (byte)(m_Address >> 24)
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
            if (ipString == null)
                throw new ArgumentNullException();

            ulong ipAddress = 0L;
            int lastIndex = 0;
            int shiftIndex = 0;
            ulong mask = 0x00000000000000FF;
            ulong octet = 0L;
            int length = ipString.Length;

            for (int i = 0; i < length; ++i)
            {
                // Parse to '.' or end of IP address
                if (ipString[i] == '.' || i == length - 1)
                    // If the IP starts with a '.'
                    // or a segment is longer than 3 characters or shiftIndex > last bit position throw.
                    if (i == 0 || i - lastIndex > 3 || shiftIndex > 24)
                    {
                        throw new ArgumentException();
                    }
                    else
                    {
                        i = i == length - 1 ? ++i : i;
                        octet = ulong.Parse(ipString.Substring(lastIndex, i - lastIndex)) & 0x00000000000000FF;
                        ipAddress = ipAddress + ((octet << shiftIndex) & mask);
                        lastIndex = i + 1;
                        shiftIndex = shiftIndex + 8;
                        mask = (mask << 8);
                    }
            }

            return new IPAddress((long)ipAddress);
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
            return ((byte)(m_Address)).ToString() +
                    "." +
                    ((byte)(m_Address >> 8)).ToString() +
                    "." +
                    ((byte)(m_Address >> 16)).ToString() +
                    "." +
                    ((byte)(m_Address >> 24)).ToString();
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
    }
}
