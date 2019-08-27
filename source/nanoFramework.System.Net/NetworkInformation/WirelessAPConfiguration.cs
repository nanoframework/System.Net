//
// Copyright (c) 2019 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
using System.Collections;

using System.Runtime.CompilerServices;

namespace System.Net.NetworkInformation
{
    /// <summary>
    /// Configuration of wireless network SOft AP based on the 802.11 standard.
    /// </summary>
    /// <remarks>
    /// This class is exclusive of nanoFramework and does not exist on the UWP API.
    /// </remarks>
    public class WirelessAPConfiguration
    {
        /// <summary>
        /// Contains the SSID length. The value is set to 32.
        /// </summary>
        private const int MaxApSsidLength = 32;
        /// <summary>
        /// Contains the maximum password length. The value is set to 64.
        /// </summary>
        private const int MaxApPasswordLength = 64;

        /// <summary>
        /// Minimum passwword length
        /// </summary>
        private const int MinApPasswordLength = 8;

        /// <summary>
        /// This is the configuration index as provided by the device storage manager.
        /// </summary>
        private readonly int _apConfigurationIndex;

        private readonly uint _apId;
        private AuthenticationType _apAuthentication;
        private EncryptionType _apEncryption;
        private RadioType _apRadio;
        private string _apPassword;
        private string _apSsid;
        private WirelessAPConfigFlags _apFlags;
        private byte _apChannel;
        private byte _apMaxConnections;

        /// <summary>
        /// Specifies the type of authentication used for the wireless AP.
        /// </summary>
        public AuthenticationType Authentication { get => _apAuthentication; set => _apAuthentication = value; }

        /// <summary>
        /// Specifies the type of encryption used for the wireless AP.
        /// </summary>
        public EncryptionType Encryption { get => _apEncryption; set => _apEncryption = value; }

        /// <summary>
        /// Specifies the type of radio used by the wireless network adapter.
        /// </summary>
        public RadioType Radio { get => _apRadio; set => _apRadio = value; }

        /// <summary>
        /// Contains the network passphrase used for clients to connect to Soft AP
        /// </summary>
        public string Password { get => _apPassword; set => _apPassword = value; }
        
        /// <summary>
        /// Contains the Soft AP SSID.
        /// </summary>
        public string Ssid { get => _apSsid; set => _apSsid = value; }

        /// <summary>
        /// Contains flags for the Soft AP 
        /// </summary>
        public WirelessAPConfigFlags Flags { get => _apFlags; set => _apFlags = value; }

        /// <summary>
        /// Channel to use for AP.
        /// </summary>
        public byte Channel { get => _apChannel; set => _apChannel = value; }
        
        /// <summary>
        /// Maximum number of client connections
        /// </summary>
        public byte MaxConnections { get => _apMaxConnections; set => _apMaxConnections = value; }

        /// <summary>
        /// Contains the ID of the wireless AP configuration.
        /// </summary>
        public uint Id => _apId;


        /// <summary>
        /// Initializes a new instance of the <see cref="WirelessAPConfiguration"/> class.
        /// </summary>
        /// <param name="id">The ID of the wireless configuration.</param>
        public WirelessAPConfiguration(uint id)
        {
            _apId = id;

            Authentication = AuthenticationType.Open;
            Encryption = EncryptionType.None;
            Radio = RadioType.NotSpecified;
            Password = null;
            Ssid = null;
            Flags = WirelessAPConfigFlags.Enable | WirelessAPConfigFlags.AutoStart;
            Channel = 8;
            MaxConnections = 1;
        }

        /// <summary>
        /// Validate and save the wireless Soft AP configuration information.
        /// </summary>
        /// <remarks>
        /// Checks the length of SSID is 32 or less.
        /// Password length is between 8 and 64 if not an open Authentication.
        /// </remarks>
        public void SaveConfiguration()
        {
            // Before we update validate whether settings conform to right characteristics.
            ValidateConfiguration();

            UpdateConfiguration();
        }

        private void ValidateConfiguration()
        {
            // SSID can't be null
            if (_apSsid == null)
            {
                throw new ArgumentNullException();
            }

            // Check SSID length
            if (_apSsid.Length >= MaxApSsidLength)
            {
                throw new ArgumentOutOfRangeException();
            }

            // If not using an open Auth then check password length
            if ( (Authentication != AuthenticationType.Open && Authentication != AuthenticationType.None)  &&
                 ( (_apPassword.Length <  MinApPasswordLength) || (_apSsid.Length >= MaxApSsidLength) ) )
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Retrieves an array of all of the wireless Soft AP network configurations.
        /// </summary>
        /// <returns>An array containing all of the wireless 802.11 AP network configuration stored in the device. </returns>
        public static WirelessAPConfiguration[] GetAllWirelessAPConfigurations()
        {
            int count = GetWirelessAPConfigurationCount();
            WirelessAPConfiguration[] configurations = new WirelessAPConfiguration[count];

            for (int i = 0; i < count; i++)
            {
                configurations[i] = GetWirelessAPConfiguration(i);
            }

            return configurations;

        }

        /// <summary>
        /// Returns an array of connected stations.
        /// </summary>
        /// <param name="stationIndex">The index of station to get Info or 0 to return info on all connected stations.</param>
        /// <returns>An arry of WirelessAPStation</returns>
        public WirelessAPStation[] GetConnectedStations(int stationIndex)
        {
            return NativeGetConnectedClients(stationIndex);
        }

        /// <summary>
        /// DeAuthorise a connected station
        /// </summary>
        /// <param name="stationIndex">The index of station to De-Auth or 0 to De-Auth all stations.</param>
        public void DeAuthStation(int stationIndex)
        {
            NativeDeauthStation(stationIndex);
        }

        #region enums

        /// <summary>
        /// Specifies the authentication used in a wireless network.
        /// </summary>
        public enum AuthenticationType : byte
        {
            /// <summary>
            /// No protocol.
            /// </summary>
            None = 0,
            /// <summary>
            /// Extensible Authentication Protocol.
            /// </summary>
            EAP,
            /// <summary>
            /// Protected Extensible Authentication Protocol.
            /// </summary>
            PEAP,
            /// <summary>
            /// Microsoft Windows Connect Now protocol.
            /// </summary>
            WCN,
            /// <summary>
            /// Open System authentication, for use with WEP encryption type.
            /// </summary>
            Open,
            /// <summary>
            /// Shared Key authentication, for use with WEP encryption type.
            /// </summary>
            Shared,
            /// <summary>
            /// Wired Equivalent Privacy protocol.
            /// </summary>
            WEP,
            /// <summary>
            /// Wi-Fi Protected Access protocol.
            /// </summary>
            WPA,
            /// <summary>
            /// Wi-Fi Protected Access 2 protocol.
            /// </summary>
            WPA2,
        }

        /// <summary>
        /// Defines the available types of encryption for wireless networks.
        /// </summary>
        public enum EncryptionType : byte
        {
            /// <summary>
            /// No encryption.
            /// </summary>
            None = 0,
            /// <summary>
            /// Wired Equivalent Privacy encryption.
            /// </summary>
            WEP,
            /// <summary>
            /// Wireless Protected Access encryption.
            /// </summary>
            WPA,
            /// <summary>
            /// Wireless Protected Access 2 encryption.
            /// </summary>
            WPA2,
            /// <summary>
            /// Wireless Protected Access Pre-Shared Key encryption.
            /// </summary>
            WPA_PSK,
            /// <summary>
            /// Wireless Protected Access 2 Pre-Shared Key encryption.
            /// </summary>
            WPA2_PSK,
            /// <summary>
            /// Certificate encryption.
            /// </summary>
            Certificate,
        }

        /// <summary>
        /// Specifies the type of radio that the wireless network uses.
        /// </summary>
        public enum RadioType : byte
        {
            /// <summary>
            /// Not specified.
            /// </summary>
            NotSpecified = 0,
            /// <summary>
            /// 802.11a-compatible radio.
            /// </summary>
            _802_11a = 1,
            /// <summary>
            /// 802.11b-compatible radio.
            /// </summary>
            _802_11b = 2,
            /// <summary>
            /// 802.11g-compatible radio.
            /// </summary>
            _802_11g = 4,
            /// <summary>
            /// 802.11n-compatible radio.
            /// </summary>
            _802_11n = 8,
        }

        #endregion

        #region native methods

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static int GetWirelessAPConfigurationCount();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static WirelessAPConfiguration GetWirelessAPConfiguration(int configurationIndex);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static void UpdateConfiguration();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static WirelessAPStation[] NativeGetConnectedClients(int ClientIndex);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static string NativeDeauthStation(int ClientIndex);
        #endregion
    }
}
