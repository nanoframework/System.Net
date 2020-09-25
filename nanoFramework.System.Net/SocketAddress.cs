//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net
{
    using System.Net.Sockets;

    /// <summary>
    /// Stores serialized information from <see cref="EndPoint"/> derived classes.
    /// </summary>
    public class SocketAddress
    {
        internal const int IPv6AddressSize = 28;
        internal const int IPv4AddressSize = 16;

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        internal byte[] m_Buffer;

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        internal long _address;

        /// <summary>
        /// Gets the address family for the current address.
        /// </summary>
        /// <value>A value specifying the addressing scheme that is used to resolve the current address.</value>
        public AddressFamily Family
        {
            get
            {
                return (AddressFamily)(m_Buffer[0] | (m_Buffer[1] << 8));
            }
        }

        internal SocketAddress(byte[] address)
        {
            m_Buffer = address;
        }

        internal SocketAddress(IPAddress ipAddress)
            :this(ipAddress.AddressFamily,
                (ipAddress.AddressFamily == AddressFamily.InterNetwork) ? IPv4AddressSize : IPv6AddressSize)

        {
            // no Port
            m_Buffer[2] = 0;
            m_Buffer[3] = 0;

            //if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            //{
            //    // No handling for Flow Information
            //    m_Buffer[4] = 0;
            //    m_Buffer[5] = 0;
            //    m_Buffer[6] = 0;
            //    m_Buffer[7] = 0;

            //    // Scope serialization
            //    long scope = ipAddress.ScopeId;
            //    m_Buffer[24] = (byte)scope;
            //    m_Buffer[25] = (byte)(scope >> 8);
            //    m_Buffer[26] = (byte)(scope >> 16);
            //    m_Buffer[27] = (byte)(scope >> 24);

            //    // Address serialization
            //    byte[] addressBytes = ipAddress.GetAddressBytes();
            //    for (int i = 0; i < addressBytes.Length; i++)
            //    {
            //        m_Buffer[8 + i] = addressBytes[i];
            //    }
            //}
            //else
            {
                // IPv4 Address serialization
                m_Buffer[4] = unchecked((byte)(ipAddress._address));
                m_Buffer[5] = unchecked((byte)(ipAddress._address >> 8));
                m_Buffer[6] = unchecked((byte)(ipAddress._address >> 16));
                m_Buffer[7] = unchecked((byte)(ipAddress._address >> 24));
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SocketAddress"/> class using the specified address family and buffer size.
        /// </summary>
        /// <param name="family">An <see cref="AddressFamily"/> enumerated value.</param>
        /// <param name="size">The number of bytes to allocate for the underlying buffer.</param>
        /// <remarks>
        /// Use this overload to create a new instance of the <see cref="SocketAddress"/> class with a particular underlying buffer size.
        /// </remarks>
        public SocketAddress(AddressFamily family, int size)
        {
           // Debug.Assert(size > 2);

            m_Buffer = new byte[size]; //(size / IntPtr.Size + 2) * IntPtr.Size];//sizeof DWORD

            m_Buffer[0] = unchecked((byte)((int)family     ));
            m_Buffer[1] = unchecked((byte)((int)family >> 8));
        }

        /// <summary>
        /// Gets the underlying buffer size of the <see cref="SocketAddress"/>.
        /// </summary>
        /// <value>
        /// The underlying buffer size of the <see cref="SocketAddress"/>.
        /// </value>
        /// <remarks>
        /// This property gets the underlying buffer size of the <see cref="SocketAddress"/> in bytes.
        /// </remarks>
        public int Size
        {
            get { return m_Buffer.Length; }
        }

        /// <summary>
        /// Gets or sets the specified index element in the underlying buffer.
        /// </summary>
        /// <param name="offset">The array index element of the desired information.</param>
        /// <value>The value of the specified index element in the underlying buffer.</value>
        /// <remarks>
        /// This property gets or sets the specified byte position in the underlying buffer.
        /// </remarks>
        public byte this[int offset]
        {
            get { return m_Buffer[offset]; }
            set { m_Buffer[offset] = value; }
        }

        internal IPEndPoint GetIPEndPoint()
        {
            IPAddress address = GetIPAddress();
            int port = (int)((m_Buffer[2] << 8 & 0xFF00) | (m_Buffer[3]));
            return new IPEndPoint(address, port);
        }

        internal IPAddress GetIPAddress()
        {
            //if (Family == AddressFamily.InterNetworkV6)
            //{
            //    byte[] address = new byte[IPAddress.IPv6AddressBytes];
            //    for (int i = 0; i < address.Length; i++)
            //    {
            //        address[i] = m_Buffer[i + 8];
            //    }

            //    long scope = (long)((m_Buffer[27] << 24) +
            //                        (m_Buffer[26] << 16) +
            //                        (m_Buffer[25] << 8) +
            //                        (m_Buffer[24]));

            //    return new IPAddress(address, scope);

            //}
            //else if (Family == AddressFamily.InterNetwork)
            {
                long address = (long)(
                        (m_Buffer[4] & 0x000000FF) |
                        (m_Buffer[5] << 8 & 0x0000FF00) |
                        (m_Buffer[6] << 16 & 0x00FF0000) |
                        (m_Buffer[7] << 24)
                        ) & 0x00000000FFFFFFFF;

                return new IPAddress(address);
            }
        }
    }
}
