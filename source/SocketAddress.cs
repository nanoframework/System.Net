////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{
    using System.Net.Sockets;

    /// <summary>
    /// Stores serialized information from <see cref="EndPoint"/> derived classes.
    /// </summary>
    public class SocketAddress
    {
        internal const int IPv4AddressSize = 16;

        internal byte[] m_Buffer;

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

    } // class SocketAddress
} // namespace System.Net


