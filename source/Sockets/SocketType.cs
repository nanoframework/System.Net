////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net.Sockets
{
    /// <summary>
    /// Specifies the type of socket an instance of the <see cref='System.Net.Sockets.Socket'/> class represents.
    /// </summary>
    /// <remarks>
    /// Before a <see cref="Socket"/> can send and receive data, it must first be created using an <see cref="AddressFamily"/>, a SocketType, 
    /// and a ProtocolType. The SocketType enumeration provides several options for defining the type of <see cref="Socket"/> that you 
    /// intend to open.
    /// </remarks>
    public enum SocketType
    {
        /// <summary>
        /// Supports reliable, two-way, connection-based byte streams without the duplication of data and without preservation of boundaries. 
        /// A Socket of this type communicates with a single peer and requires a remote host connection before communication can 
        /// begin. Stream uses the Transmission Control Protocol (Tcp) <see cref='ProtocolType'/> and the InterNetworkAddressFamily.
        /// </summary>
        Stream = 1,    // stream socket

        /// <summary>
        /// Supports datagrams, which are connectionless, unreliable messages of a fixed (typically small) maximum length. 
        /// Messages might be lost or duplicated and might arrive out of order. A Socket of type Dgram requires no connection 
        /// prior to sending and receiving data, and can communicate with multiple peers. Dgram uses the Datagram Protocol (Udp) 
        /// and the InterNetworkAddressFamily.
        /// </summary>
        Dgram = 2,    // datagram socket

        /// <summary>
        /// Supports access to the underlying transport protocol. Using the SocketTypeRaw, you can communicate using protocols 
        /// like Internet Control Message Protocol (Icmp) and Internet Group Management Protocol (Igmp). Your application must 
        /// provide a complete IP header when sending. Received datagrams return with the IP header and options intact.
        /// </summary>
        Raw = 3,    // raw-protocolinterface
        
        /// <summary>
        /// Supports connectionless, message-oriented, reliably delivered messages, and preserves message boundaries in data. 
        /// Rdm (Reliably Delivered Messages) messages arrive unduplicated and in order. Furthermore, the sender is notified 
        /// if messages are lost. If you initialize a Socket using Rdm, you do not require a remote host connection before 
        /// sending and receiving data. With Rdm, you can communicate with multiple peers.
        /// </summary>
        Rdm = 4,    // reliably-delivered message
        
        /// <summary>
        /// Provides connection-oriented and reliable two-way transfer of ordered byte streams across a network. Seqpacket 
        /// does not duplicate data, and it preserves boundaries within the data stream. A Socket of type Seqpacket 
        /// communicates with a single peer and requires a remote host connection before communication can begin.
        /// </summary>
        Seqpacket = 5,    // sequenced packet stream
        
        /// <summary>
        /// Specifies an unknown Socket type.
        /// </summary>
        Unknown = -1,   // Unknown socket type

    } // enum SocketType

} // namespace System.Net.Sockets


