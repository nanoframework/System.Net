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
    public enum PhyProtocol
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
    /// Class that encalsulates the details of a connected client.
    /// </summary>
    public class WirelessAPStation
    {
        private byte[] _MacAddress;
        private sbyte _Rssi;
        private PhyProtocol _PhyModes;

        internal WirelessAPStation(byte[] mac, sbyte rssi, PhyProtocol phyp)
        {
            _MacAddress = mac;
            _Rssi = rssi;
            _PhyModes = phyp;
        }

        /// <summary>
        /// Returns the MAc address of teh connected Client.
        /// </summary>
        public byte[] MacAddres { get => _MacAddress;  }

        /// <summary>
        /// Returns the Received signal strength indication(RSSI) of connected Client.
        /// RSSI is a value from 0 to 127 where teh higher teh number the stronger teh signal.
        /// </summary>
        public sbyte Rssi { get => _Rssi; }

        /// <summary>
        /// Returns the PHY protcol used for connection.
        /// </summary>
        public PhyProtocol PhyModes { get => _PhyModes; }

    }
}
