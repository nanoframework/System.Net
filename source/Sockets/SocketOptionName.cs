//
// Copyright (c) 2018 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Sockets
{
    using System;

    /// <summary>
    /// Defines socket option names for the <see cref='System.Net.Sockets.Socket'/> class.
    /// </summary>
    /// <remarks>
    /// The SocketOptionName enumeration defines the name of each Socket configuration option. 
    /// Sockets can be configured with the <see cref="Socket.SetSocketOption(SocketOptionLevel, SocketOptionName, Boolean)"/> method.
    /// </remarks>
    public enum SocketOptionName
    {

        //
        // Used for SocketOptionLevel.Socket
        //

        /// <summary>
        /// <para>Record debugging information.</para>
        /// </summary>
        Debug = 0x0001,           // turn on debugging info recording
 
        /// <summary>
        /// <para>Socket is listening.</para>
        /// </summary>
        AcceptConnection = 0x0002,           // socket has had listen()
 
        /// <summary>
        /// Allows the socket to be bound to an address that is already in use.
        /// </summary>
        ReuseAddress = 0x0004,           // allow local address reuse
 
        /// <summary>
        /// Send keep-alives.
        /// </summary>
        KeepAlive = 0x0008,           // keep connections alive

        /// <summary>
        ///  Do not route, send directly to interface addresses.
        /// </summary>
        DontRoute = 0x0010,           // just use interface addresses

        /// <summary>
        /// Permit sending broadcast messages on the socket.
        /// </summary>
        Broadcast = 0x0020,           // permit sending of broadcast msgs
 
        /// <summary>
        /// Bypass hardware when possible.
        /// </summary>
        UseLoopback = 0x0040,           // bypass hardware when possible

        /// <summary>
        /// Linger on close if unsent data is present.
        /// </summary>
        Linger = 0x0080,           // linger on close if data present
 
        /// <summary>
        /// Receives out-of-band data in the normal data stream.
        /// </summary>
        OutOfBandInline = 0x0100,           // leave received OOB data in line
 
        /// <summary>
        /// Close socket gracefully without lingering.
        /// </summary>
        DontLinger = ~Linger,
 
        /// <summary>
        ///  Enables a socket to be bound for exclusive access.
        /// </summary>
        ExclusiveAddressUse = ~ReuseAddress,    // disallow local address reuse
 
        /// <summary>
        /// Specifies the total per-socket buffer space reserved for sends. This is
        /// unrelated to the maximum message size or the size of a TCP window.
        /// </summary>
        SendBuffer = 0x1001,           // send buffer size
 
        /// <summary>
        /// Send low water mark.
        /// </summary>
        ReceiveBuffer = 0x1002,           // receive buffer size

        /// <summary>
        /// Specifies the total per-socket buffer space reserved for receives. 
        /// This is unrelated to the maximum message size or the size of a TCP window.
        /// </summary>
        SendLowWater = 0x1003,           // send low-water mark
 
        /// <summary>
        /// Receive low water mark.
        /// </summary>
        ReceiveLowWater = 0x1004,           // receive low-water mark
 
        /// <summary>
        /// Send timeout.
        /// </summary>
        SendTimeout = 0x1005,           // send timeout
 
        /// <summary>
        /// Receive timeout.
        /// </summary>
        ReceiveTimeout = 0x1006,           // receive timeout
 
        /// <summary>
        /// Get error status and clear.
        /// </summary>
        Error = 0x1007,          // get error status and clear
 
        /// <summary>
        /// Get socket type.
        /// </summary>
        Type = 0x1008,           // get socket type
 
        /// <summary>
        /// Maximum queue length that can be specified by <see cref='System.Net.Sockets.Socket.Listen'/>.
        /// </summary>
        MaxConnections = 0x7fffffff,       // Maximum queue length specifiable by listen.

        //
        // the following values are taken from ws2tcpip.h,
        // note that these are understood only by ws2_32.dll and are not backwards compatible
        // with the values found in winsock.h which are understood by wsock32.dll.
        //

        //
        // good for SocketOptionLevel.IP
        //

        /// <summary>
        /// IP options.
        /// </summary>
        IPOptions = 1,
 
        /// <summary>
        /// Header is included with data.
        /// </summary>
        HeaderIncluded = 2,

        /// <summary>
        ///  IP type of service and preced.
        /// </summary>
        TypeOfService = 3,
 
        /// <summary>
        /// IP time to live.
        /// </summary>
        IpTimeToLive = 4,

        /// <summary>
        ///    <para>
        ///       IP multicast interface.
        ///       - Additional comments by mbolien:
        ///         multicast interface  You provide it with an SOCKADDR_IN, and that tells the
        ///         system that it should receive multicast messages on that interface (if you
        ///         have more than one interface).  Binding the socket is not sufficient, since
        ///         if the Ethernet hardware isnt set up to grab the multicast packets, it wont
        ///         do good to bind the socket.  Kinda like raw sockets.  Unless you
        ///         put the Ethernet card in promiscuous mode, youll only get stuff sent to and
        ///         from your machine.
        ///    </para>
        /// </summary>
        MulticastInterface = 9,
 
        /// <summary>
        /// IP multicast time to live.
        /// </summary>
        MulticastTimeToLive = 10,

        /// <summary>
        /// IP Multicast loopback.
        /// </summary>
        MulticastLoopback = 11,
 
        /// <summary>
        /// Add an IP group membership.
        /// </summary>
        AddMembership = 12,

        /// <summary>
        /// Drop an IP group membership.
        /// </summary>
        DropMembership = 13,

        /// <summary>
        /// Don't fragment IP datagrams.
        /// </summary>
        DontFragment = 14,

        /// <summary>
        /// Join IP group/source.
        /// </summary>
        AddSourceMembership = 15,

        /// <summary>
        ///  Leave IP group/source.
        /// </summary>
        DropSourceMembership = 16,

        /// <summary>
        /// Block IP group/source.
        /// </summary>
        BlockSource = 17,
 
        /// <summary>
        /// Unblock IP group/source.
        /// </summary>
        UnblockSource = 18,
 
        /// <summary>
        /// Receive packet information for ipv4.
        /// </summary>
        PacketInformation = 19,

        //
        //good for ipv6
        //

        /// <summary>
        /// Specifies the maximum number of router hops for an Internet Protocol version 6 (IPv6) packet. 
        /// This is similar to Time to Live (TTL) for Internet Protocol version 4.
        /// </summary>
        HopLimit = 21,            //IPV6_HOPLIMIT
        //
        // good for SocketOptionLevel.Tcp
        //

        /// <summary>
        /// Disables the Nagle algorithm for send coalescing.
        /// </summary>
        NoDelay = 1,

        /// <summary>
        /// Use urgent data as defined in RFC-1222. This option can be set only once; after it is set, it cannot be turned off.
        /// </summary>
        BsdUrgent = 2,

        /// <summary>
        /// Use expedited data as defined in RFC-1222. This option can be set only once; after it is set, it cannot be turned off.
        /// </summary>
        Expedited = 2,

        //
        // good for SocketOptionLevel.Udp
        //

        /// <summary>
        /// Send UDP datagrams with checksum set to zero.
        /// </summary>
        NoChecksum = 1,

        /// <summary>
        /// Set or get the UDP checksum coverage.
        /// </summary>
        ChecksumCoverage = 20,


        /// <summary>
        /// Updates an accepted socket's properties by using those of an existing socket. This is equivalent to using the Winsock2 SO_UPDATE_ACCEPT_CONTEXT 
        /// socket option and is supported only on connection-oriented sockets.
        /// </summary>
        UpdateAcceptContext = 0x700B,

        /// <summary>
        /// Updates a connected socket's properties by using those of an existing socket. This is equivalent to using the Winsock2 SO_UPDATE_CONNECT_CONTEXT 
        /// socket option and is supported only on connection-oriented sockets.
        /// </summary>
        UpdateConnectContext = 0x7010,

    }
}
