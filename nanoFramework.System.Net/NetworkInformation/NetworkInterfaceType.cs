﻿//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.NetworkInformation
{
    /// <summary>
    /// Specifies the type of network interface used by the device.
    /// </summary>
    /// <remarks>
    /// This Enum is exclusive of nanoFramework and it does not exist on the UWP API.
    /// </remarks>
    public enum NetworkInterfaceType
    {
        /////////////////////////////////////////////////////////////////////////////////////////////
        // NEED TO KEEP THESE IN SYNC WITH:                                                        //
        // native 'NetworkInterfaceType' enum in nanoHAL_Network.h                                 //
        // debugger lib: NetworkInterfaceType.cs                                                   //
        /////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The network interface type is unknown or not specified.
        /// </summary>
        Unknown = 1,

        /// <summary>
        /// The network interface uses an Ethernet connection. Ethernet is defined in IEEE standard 802.3.
        /// </summary>
        Ethernet = 6,

        /// <summary>
        /// The network interface uses a wireless LAN connection (IEEE 802.11 standard).
        /// </summary>
        Wireless80211 = 71,

        /// <summary>
        /// The network interface uses a wireless Soft AP connection (IEEE 802.11 standard).
        /// </summary>
        WirelessAP = 72,

    }
}
