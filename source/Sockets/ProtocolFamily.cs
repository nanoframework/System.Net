////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net.Sockets
{
    /// <summary>
    /// Specifies the type of protocol that an instance of the <see cref='System.Net.Sockets.Socket'/>
    /// </summary>
    public enum ProtocolFamily
    {
        /// <summary>
        /// Unknown protocol.
        /// </summary>
        Unknown = AddressFamily.Unknown,
        /// <summary>
        /// Unspecified protocol.
        /// </summary>
        Unspecified = AddressFamily.Unspecified,
        /// <summary>
        /// Unix local to host protocol.
        /// </summary>
        Unix = AddressFamily.Unix,
        /// <summary>
        /// IP version 4 protocol.
        /// </summary>
        InterNetwork = AddressFamily.InterNetwork,
        /// <summary>
        /// ARPANET IMP protocol.
        /// </summary>
        ImpLink = AddressFamily.ImpLink,
        /// <summary>
        /// PUP protocol.
        /// </summary>
        Pup = AddressFamily.Pup,
        /// <summary>
        /// MIT CHAOS protocol.
        /// </summary>
        Chaos = AddressFamily.Chaos,
        /// <summary>
        /// Xerox NS protocol.
        /// </summary>
        NS = AddressFamily.NS,
        /// <summary>
        /// IPX or SPX protocol.
        /// </summary>
        Ipx = AddressFamily.Ipx,
        /// <summary>
        /// ISO protocol.
        /// </summary>
        Iso = AddressFamily.Iso,
        /// <summary>
        /// OSI protocol.
        /// </summary>
        Osi = AddressFamily.Osi,
        /// <summary>
        /// European Computer Manufacturers Association (ECMA) protocol.
        /// </summary>
        Ecma = AddressFamily.Ecma,
        /// <summary>
        /// DataKit protocol.
        /// </summary>
        DataKit = AddressFamily.DataKit,
        /// <summary>
        /// CCITT protocol, such as X.25.
        /// </summary>
        Ccitt = AddressFamily.Ccitt,
        /// <summary>
        /// IBM SNA protocol.
        /// </summary>
        Sna = AddressFamily.Sna,
        /// <summary>
        /// DECNet protocol.
        /// </summary>
        DecNet = AddressFamily.DecNet,
        /// <summary>
        /// Direct data link protocol.
        /// </summary>
        DataLink = AddressFamily.DataLink,
        /// <summary>
        /// LAT protocol.
        /// </summary>
        Lat = AddressFamily.Lat,
        /// <summary>
        /// NSC HyperChannel protocol.
        /// </summary>
        HyperChannel = AddressFamily.HyperChannel,
        /// <summary>
        /// AppleTalk protocol.
        /// </summary>
        AppleTalk = AddressFamily.AppleTalk,
        /// <summary>
        /// NetBIOS protocol.
        /// </summary>
        NetBios = AddressFamily.NetBios,
        /// <summary>
        /// VoiceView protocol.
        /// </summary>
        VoiceView = AddressFamily.VoiceView,
        /// <summary>
        /// FireFox protocol.
        /// </summary>
        FireFox = AddressFamily.FireFox,
        /// <summary>
        /// Banyan protocol.
        /// </summary>
        Banyan = AddressFamily.Banyan,
        /// <summary>
        /// Native ATM services protocol.
        /// </summary>
        Atm = AddressFamily.Atm,
        /// <summary>
        /// IP version 6 protocol.
        /// </summary>
        InterNetworkV6 = AddressFamily.InterNetworkV6,
        /// <summary>
        /// Microsoft Cluster products protocol.
        /// </summary>
        Cluster = AddressFamily.Cluster,
        /// <summary>
        /// IEEE 1284.4 workgroup protocol.
        /// </summary>
        Ieee12844 = AddressFamily.Ieee12844,
        /// <summary>
        /// IrDA protocol.
        /// </summary>
        Irda = AddressFamily.Irda,
        /// <summary>
        /// Network Designers OSI gateway enabled protocol.
        /// </summary>
        NetworkDesigners = AddressFamily.NetworkDesigners,
        /// <summary>
        /// MAX protocol.
        /// </summary>
        Max = AddressFamily.Max,

    }; // enum ProtocolFamily

} // namespace System.Net.Sockets


