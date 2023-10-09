//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

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
        /// Minimum password length
        /// </summary>
        private const int MinApPasswordLength = 8;

#pragma warning disable IDE0032 // nanoFramework doesn't support auto-properties

        /// <summary>
        /// This is the configuration index as provided by the device storage manager.
        /// </summary>
#pragma warning disable S1144 // field used in native code
        private readonly int _apConfigurationIndex;
#pragma warning restore S1144 // Unused private types or members should be removed

        private readonly uint _apId;
        private AuthenticationType _apAuthentication;
        private EncryptionType _apEncryption;
        private RadioType _apRadio;
        private string _apPassword;
        private string _apSsid;
        private ConfigurationOptions _options;
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
        public ConfigurationOptions Options { get => _options; set => _options = value; }

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

#pragma warning restore IDE0032 // nanoFramework doesn't support auto-properties

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
            Options = ConfigurationOptions.Enable | ConfigurationOptions.AutoStart;
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
#pragma warning disable S3928 // OK to not include a meaningful message
                throw new ArgumentNullException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
            }

            // Check SSID length
            if (_apSsid.Length >= MaxApSsidLength)
            {
#pragma warning disable S3928 // OK to not include a meaningful message
                throw new ArgumentOutOfRangeException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
            }

            // At least one MaxConnection is required
            if (_apMaxConnections < 1)
            {
#pragma warning disable S3928 // OK to not include a meaningful message
                throw new ArgumentOutOfRangeException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
            }

            // If not using an open Auth then check password length
            if ( (Authentication != AuthenticationType.Open && Authentication != AuthenticationType.None)  &&
                 ( (_apPassword.Length <  MinApPasswordLength) || (_apPassword.Length >= MaxApPasswordLength) ) )
            {
#pragma warning disable S3928 // OK to not include a meaningful message
                throw new ArgumentOutOfRangeException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
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
        /// Returns an array of information about the connected stations.
        /// </summary>
        /// <returns>A <see cref="WirelessAPStation"/></returns>
        public WirelessAPStation[] GetConnectedStations()
        {
            return NativeGetConnectedClients(0);
        }

        /// <summary>
        /// Returns information about the a connected station.
        /// </summary>
        /// <param name="stationIndex">The index of station to get information about.</param>
        /// <returns>An <see cref="WirelessAPStation"/>.</returns>
        public WirelessAPStation GetConnectedStations(int stationIndex)
        {
            return NativeGetConnectedClients(stationIndex)[0];
        }

        /// <summary>
        /// DeAuthorise a connected station
        /// </summary>
        /// <param name="stationIndex">The index of station to De-Auth or 0 to De-Auth all stations.</param>
        public void DeAuthStation(int stationIndex)
        {
            NativeDeauthStation(stationIndex);
        }

        /// <summary>
        /// Configuration flags used for Wireless Soft AP configuration.
        /// </summary>
        [Flags]
        public enum ConfigurationOptions : byte
        {
            /// <summary>
            /// No option set.
            /// </summary>
            None = 0,

            /// <summary>
            /// Disables the Wireless Soft AP.
            /// </summary>
            Disable = 0x01,

            /// <summary>
            /// Enables the Wireless Soft AP.
            /// If not set the Wireless Soft AP is disabled.
            /// </summary>
            Enable = 0x02,

            /// <summary>
            /// Will automatically start the Soft AP when CLR starts.
            /// This option forces enabling the Wireless Soft AP.
            /// </summary>
            AutoStart = 0x04 | Enable,

            /// <summary>
            /// The SSID for the Soft AP will be hidden.
            /// </summary>
            HiddenSSID = 0x08,
        };

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
