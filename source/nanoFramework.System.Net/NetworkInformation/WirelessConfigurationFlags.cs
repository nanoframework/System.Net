//
// Copyright (c) 2019 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System;

namespace System.Net.NetworkInformation
{
    /// <summary>
    /// Configuration flags used for Wireless configuration.
    /// </summary>
    [Flags]
    public enum WirelessConfigFlags : byte
    {
        /// <summary>
        /// No flags set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Enables the Wireless station.
        /// If not set the wireless station is disabled.
        /// </summary>
        Enable = 0x01,

        /// <summary>
        /// Will auto connect when AP is available or after being disconnected.
        /// </summary>
        AutoConnect = 0x02,

        /// <summary>
        /// Enables Smart config(if available) for this Wireless station
        /// </summary>
        SmartConfig = 0x04,
    };

    /// <summary>
    /// Configuration flags used for Wireless Soft AP configuration.
    /// </summary>
    [Flags]
    public enum WirelessAPConfigFlags : byte
    {
        /// <summary>
        /// No flags set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Enables the Wireless Soft AP.
        /// If not set the wireless Soft AP is disabled.
        /// </summary>
        Enable = 0x01,

        /// <summary>
        /// Will automatically start AP when CLR starts.
        /// </summary>
        AutoStart = 0x02,

        /// <summary>
        /// The SSID for the Soft AP will be hidden 
        /// </summary>
        HiddenSSID = 0x04,
    };

}
