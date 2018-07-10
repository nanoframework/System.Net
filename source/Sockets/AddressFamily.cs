////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net.Sockets
{
    /// <summary>
    ///  Specifies the address families that an instance of the <see cref='System.Net.Sockets.Socket'/>
    /// </summary>
    /// <remarks>
    /// An AddressFamily member specifies the addressing scheme that a <see cref="Socket"/> will use to resolve an address. For example, 
    /// InterNetwork indicates that an IP version 4 address is expected when a <see cref="Socket"/> connects to an endpoint.
    /// </remarks>
    public enum AddressFamily
    {
        /// <summary>
        /// Unknown address family.
        /// </summary>
        Unknown = -1,   // Unknown
        /// <summary>
        /// Unspecified address family.
        /// </summary>
        Unspecified = 0,    // unspecified
        /// <summary>
        /// Unix local to host address.
        /// </summary>
        Unix = 1,    // local to host (pipes, portals)
        /// <summary>
        /// Address for IP version 4.
        /// </summary>
        InterNetwork = 2,    // internetwork: UDP, TCP, etc.
        /// <summary>
        /// ARPANET IMP address.
        /// </summary>
        ImpLink = 3,    // arpanet imp addresses
        /// <summary>
        /// Address for PUP protocols.
        /// </summary>
        Pup = 4,    // pup protocols: e.g. BSP
        /// <summary>
        /// Address for MIT CHAOS protocols.
        /// </summary>
        Chaos = 5,    // mit CHAOS protocols
        /// <summary>
        /// Address for Xerox NS protocols.
        /// </summary>
        NS = 6,    // XEROX NS protocols
        /// <summary>
        /// IPX or SPX address.
        /// </summary>
        Ipx = NS,   // IPX and SPX
        /// <summary>
        /// Address for ISO protocols.
        /// </summary>
        Iso = 7,    // ISO protocols
        /// <summary>
        /// Address for OSI protocols.
        /// </summary>
        Osi = Iso,  // OSI is ISO
        /// <summary>
        /// European Computer Manufacturers Association (ECMA) address.
        /// </summary>
        Ecma = 8,    // european computer manufacturers
        /// <summary>
        /// Address for Datakit protocols.
        /// </summary>
        DataKit = 9,    // datakit protocols
        /// <summary>
        /// Addresses for CCITT protocols, such as X.25.
        /// </summary>
        Ccitt = 10,   // CCITT protocols, X.25 etc
        /// <summary>
        /// IBM SNA address.
        /// </summary>
        Sna = 11,   // IBM SNA
        /// <summary>
        /// DECnet address.
        /// </summary>
        DecNet = 12,   // DECnet
        /// <summary>
        /// Direct data-link interface address.
        /// </summary>
        DataLink = 13,   // Direct data link interface
        /// <summary>
        /// LAT address.
        /// </summary>
        Lat = 14,   // LAT
        /// <summary>
        /// NSC Hyperchannel address.
        /// </summary>
        HyperChannel = 15,   // NSC Hyperchannel
        /// <summary>
        /// AppleTalk address.
        /// </summary>
        AppleTalk = 16,   // AppleTalk
        /// <summary>
        /// NetBios address.
        /// </summary>
        NetBios = 17,   // NetBios-style addresses
        /// <summary>
        /// VoiceView address.
        /// </summary>
        VoiceView = 18,   // VoiceView
        /// <summary>
        /// FireFox address.
        /// </summary>
        FireFox = 19,   // FireFox
        /// <summary>
        /// Banyan address.
        /// </summary>
        Banyan = 21,   // Banyan
        /// <summary>
        /// Native ATM services address.
        /// </summary>
        Atm = 22,   // Native ATM Services
        /// <summary>
        /// Internetwork Version 6.
        /// </summary>
        InterNetworkV6 = 23,
        /// <summary>
        /// Address for Microsoft cluster products.
        /// </summary>
        Cluster = 24,   // Microsoft Wolfpack
        /// <summary>
        /// IEEE 1284.4 workgroup address.
        /// </summary>
        Ieee12844 = 25,   // IEEE 1284.4 WG AF
        /// <summary>
        /// IrDA address.
        /// </summary>
        Irda = 26,   // IrDA
        /// <summary>
        /// Address for Network Designers OSI gateway-enabled protocols.
        /// </summary>
        NetworkDesigners = 28,   // Network Designers OSI & gateway enabled protocols
        /// <summary>
        /// MAX address.
        /// </summary>
        Max = 29,   // Max
    }; // enum AddressFamily
} // namespace System.Net.Sockets


