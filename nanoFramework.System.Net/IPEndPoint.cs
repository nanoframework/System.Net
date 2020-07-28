// namespace System.Net
//
// Copyright (c) 2018 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net
{
    using System.Net.Sockets;

    /// <summary>
    /// Represents a network endpoint as an IP address and a port number.
    /// </summary>
    [Serializable]
    public class IPEndPoint : EndPoint
    {
        /// <summary>
        /// Specifies the minimum value that can be assigned to the Port property. This field is read-only.
        /// </summary>
        public const int MinPort = 0x00000000;
        
        /// <summary>
        /// Specifies the maximum value that can be assigned to the Port property. The MaxPort value is set to 0x0000FFFF. This field is read-only.
        /// </summary>
        public const int MaxPort = 0x0000FFFF;

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private IPAddress _address;

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private int _port;

        /// <summary>
        /// Initializes a new instance of the <see cref="IPEndPoint"/> class with the specified address and port number.
        /// </summary>
        /// <param name="address">The IP address of the Internet host.</param>
        /// <param name="port">The port number associated with the address, or 0 to specify any available port. port is in host order.</param>
        public IPEndPoint(long address, int port)
        {
            _port = port;
            _address = new IPAddress(address);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPEndPoint"/> class with the specified address and port number.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public IPEndPoint(IPAddress address, int port)
        {
            _port = port;
            _address = address;
        }

        /// <summary>
        /// Gets or sets the IP address of the endpoint.
        /// </summary>
        /// <value>An IPAddress instance containing the IP address of the endpoint.</value>
        public IPAddress Address
        {
            get
            {
                return _address;
            }
        }

        /// <summary>
        /// Gets the Internet Protocol (IP) address family.
        /// </summary>
        /// <value>Returns <see cref="AddressFamily.InterNetwork"/>.</value>
        public AddressFamily AddressFamily
        {
            get
            {
                return _address.AddressFamily;
            }
        }

        /// <summary>
        /// Gets or sets the port number of the endpoint.
        /// </summary>
        /// <value>An integer value in the range <see cref="MinPort"/> to <see cref="MaxPort"/> indicating the port number of the endpoint.</value>
        public int Port
        {
            get
            {
                return _port;
            }
        }

        /// <summary>
        /// Serializes endpoint information into a <see cref="SocketAddress"/> instance.
        /// </summary>
        /// <returns>A <see cref="SocketAddress"/> instance containing the socket address for the endpoint.</returns>
        public override SocketAddress Serialize()
        {
            // create a new SocketAddress
            //
            SocketAddress socketAddress = new SocketAddress(AddressFamily.InterNetwork, SocketAddress.IPv4AddressSize);
            byte[] buffer = socketAddress.m_Buffer;
            //
            // populate it
            //
            buffer[2] = unchecked((byte)(_port >> 8));
            buffer[3] = unchecked((byte)(_port));

            buffer[4] = unchecked((byte)(_address._address));
            buffer[5] = unchecked((byte)(_address._address >> 8));
            buffer[6] = unchecked((byte)(_address._address >> 16));
            buffer[7] = unchecked((byte)(_address._address >> 24));

            return socketAddress;
        }

        /// <summary>
        /// Creates an endpoint from a socket address.
        /// </summary>
        /// <param name="socketAddress">The <see cref="SocketAddress"/> to use for the endpoint.</param>
        /// <returns>An <see cref="EndPoint"/> instance using the specified socket address.</returns>
        public override EndPoint Create(SocketAddress socketAddress)
        {
            // strip out of SocketAddress information on the EndPoint
            //

            byte[] buf = socketAddress.m_Buffer;

            //           Debug.Assert(socketAddress.Family == AddressFamily.InterNetwork);

            int port = (int)(
                    (buf[2] << 8 & 0xFF00) |
                    (buf[3])
                    );

            long address = (long)(
                    (buf[4] & 0x000000FF) |
                    (buf[5] << 8 & 0x0000FF00) |
                    (buf[6] << 16 & 0x00FF0000) |
                    (buf[7] << 24)
                    ) & 0x00000000FFFFFFFF;

            return new IPEndPoint(address, port);
        }

        /// <summary>
        /// Returns the IP address and port number of the specified endpoint.
        /// </summary>
        /// <returns>A string containing the IP address and the port number of the specified endpoint (for example, 192.168.1.2:80).</returns>
        public override string ToString()
        {
            return _address.ToString() + ":" + _port.ToString();
        }

        /// <summary>
        /// Determines whether the specified Object is equal to the current IPEndPoint instance.
        /// </summary>
        /// <param name="obj">The specified Object to compare with the current <see cref="IPEndPoint"/> instance.</param>
        /// <returns>true if the objects are equal.</returns>
        public override bool Equals(object obj)
        {
            IPEndPoint ep = obj as IPEndPoint;
            if (ep == null)
            {
                return false;
            }

            return ep._address.Equals(_address) && ep._port == _port;
        }

        // For security, we need to be able to take an IPEndPoint and make a copy that's immutable and not derived.
        internal IPEndPoint Snapshot()
        {
            return new IPEndPoint(Address.Snapshot(), Port);
        }
    }
}
