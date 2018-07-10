////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net.Sockets
{
    using System;

    //
    // Option flags per-socket.
    //

    /// <summary>
    /// Defines socket option levels for the <see cref='System.Net.Sockets.Socket'/> class.
    /// </summary>
    public enum SocketOptionLevel
    {
        /// <summary>
        /// Indicates socket options apply to the socket itself.
        /// </summary>
        Socket = 0xffff,
        /// <summary>
        /// Indicates socket options apply to IP sockets.
        /// </summary>
        IP = ProtocolType.IP,
        /// <summary>
        ///  Indicates socket options apply to IPv6 sockets.
        /// </summary>
        IPv6 = ProtocolType.IPv6,
        /// <summary>
        /// Indicates socket options apply to Tcp sockets.
        /// </summary>
        Tcp = ProtocolType.Tcp,
        /// <summary>
        /// Indicates socket options apply to Udp sockets.
        /// </summary>
        Udp = ProtocolType.Udp,

    }; // enum SocketOptionLevel

} // namespace System.Net.Sockets


