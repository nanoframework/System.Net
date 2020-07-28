//
// Copyright (c) 2018 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System;

namespace System.Net.NetworkInformation
{
    /// <summary>
    /// Enum of Phy protocols used for connection.
    /// </summary>
    [Flags]
    public enum PhyProtocols
    {
        /// <summary>
        /// IEEE 802.11b  max 11 Mbit/s
        /// </summary>
        PHY802_11b = 1,

        /// <summary>
        /// IEEE 802.11g  max 54 Mbit/s
        /// </summary>
        PHY802_11g = 2,

        /// <summary>
        /// IEEE 802.11n  max 288.8 Mbit/s for 20mhz channel or 600 for 40Mhz
        /// </summary>
        PHY802_11n = 4,

        /// <summary>
        /// Low rate enabled.
        /// </summary>
        PHY802_11lr = 8,
    };

    /// <summary>
    /// Class that encapsulates the details of a connected client.
    /// </summary>
    public class WirelessAPStation
    {
#pragma warning disable IDE0032 // nanoFramework doesn't support auto-properties

        private readonly byte[] _macAddress;
        private readonly sbyte _rssi;
        private readonly PhyProtocols _phyModes;

        internal WirelessAPStation(byte[] mac, sbyte rssi, PhyProtocols phyp)
        {
            _macAddress = mac;
            _rssi = rssi;
            _phyModes = phyp;
        }

        /// <summary>
        /// Returns the MAc address of the connected Client.
        /// </summary>
        public byte[] MacAddres { get => _macAddress;  }

        /// <summary>
        /// Returns the Received signal strength indication(RSSI) of connected Client.
        /// RSSI is a value from 0 to 127 where the higher the number the stronger the signal.
        /// </summary>
        public sbyte Rssi { get => _rssi; }

        /// <summary>
        /// Returns the PHY protocol used for connection.
        /// </summary>
        public PhyProtocols PhyModes { get => _phyModes; }

#pragma warning restore IDE0032 // nanoFramework doesn't support auto-properties

    }
}
