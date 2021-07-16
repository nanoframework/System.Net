//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Sockets
{
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
        /// Indicates socket options apply to TCP sockets.
        /// </summary>
        Tcp = ProtocolType.Tcp,
        /// <summary>
        /// Indicates socket options apply to UDP sockets.
        /// </summary>
        Udp = ProtocolType.Udp,

    }
}
