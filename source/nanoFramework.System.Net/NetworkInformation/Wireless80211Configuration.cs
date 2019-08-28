//
// Copyright (c) 2019 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Runtime.CompilerServices;

namespace System.Net.NetworkInformation
{
    /// <summary>
    /// Configuration of wireless network based on the 802.11 standard.
    /// </summary>
    /// <remarks>
    /// This class is exclusive of nanoFramework and does not exist on the UWP API.
    /// </remarks>
    public class Wireless80211Configuration
    {
        /// <summary>
        /// Contains the SSID length. The value is set to 32.
        /// </summary>
        private const int MaxSsidLength = 32;
        /// <summary>
        /// Contains the maximum password length. The value is set to 64.
        /// </summary>
        private const int MaxPasswordLength = 64;

#pragma warning disable IDE0032 // nanoFramework doesn't support auto-properties
        /// <summary>
        /// This is the configuration index as provided by the device storage manager.
        /// </summary>
        private readonly int _configurationIndex;

        private readonly uint _id;
        private AuthenticationType _authentication;
        private EncryptionType _encryption;
        private RadioType _radio;
        private string _password;
        private string _ssid;
        private WirelessConfigFlags _flags;
#pragma warning disable IDE0032 // nanoFramework doesn't support auto-properties

        /// <summary>
        /// Specifies the type of authentication used on the wireless network.
        /// </summary>
        public AuthenticationType Authentication { get => _authentication; set => _authentication = value; }

        /// <summary>
        /// Specifies the type of encryption used on the wireless network.
        /// </summary>
        public EncryptionType Encryption { get => _encryption; set => _encryption = value; }

        /// <summary>
        /// Specifies the type of radio used by the wireless network adapter.
        /// </summary>
        public RadioType Radio { get => _radio; set => _radio = value; }

        /// <summary>
        /// Contains flags for the Wireless conection 
        /// </summary>
        public WirelessConfigFlags Flags { get => _flags; set => _flags = value; }

        /// <summary>
        /// Contains the network passphrase.
        /// </summary>
        public string Password { get => _password; set => _password = value; }
        
        /// <summary>
        /// Contains the network's SSID.
        /// </summary>
        public string Ssid { get => _ssid; set => _ssid = value; }

        /// <summary>
        /// Contains the ID of the wireless configuration.
        /// </summary>
        public uint Id => _id;


        /// <summary>
        /// Initializes a new instance of the <see cref="Wireless80211Configuration"/> class.
        /// </summary>
        /// <param name="id">The ID of the wireless configuration.</param>
        public Wireless80211Configuration(uint id)
        {
            _id = id;

            Authentication = AuthenticationType.None;
            Encryption = EncryptionType.None;
            Radio = RadioType.NotSpecified;
            Password = null;
            Ssid = null;
        }

        /// <summary>
        /// Saves the wireless 802.11 configuration information.
        /// </summary>
        public void SaveConfiguration()
        {
            // Before we update validate whether settings conform to right characteristics.
            ValidateConfiguration();

            UpdateConfiguration();
        }

        private void ValidateConfiguration()
        {
            // SSID can't be null
            if (_ssid == null)
            {
                throw new ArgumentNullException();
            }

            // check password and SSID length
            if ((_password.Length    >= MaxPasswordLength) || 
                (_ssid.Length        >= MaxSsidLength))
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Retrieves an array of all of the wireless 802.11 network configurations.
        /// </summary>
        /// <returns>An array containing all of the wireless 802.11 network configuration stored in the device. </returns>
        public static Wireless80211Configuration[] GetAllWireless80211Configurations()
        {
            int count = GetWireless82011ConfigurationCount();
            Wireless80211Configuration[] configurations = new Wireless80211Configuration[count];

            for (int i = 0; i < count; i++)
            {
                configurations[i] = GetWireless82011Configuration(i);
            }

            return configurations;
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
        private extern static int GetWireless82011ConfigurationCount();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static Wireless80211Configuration GetWireless82011Configuration(int configurationIndex);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static void UpdateConfiguration();

        #endregion
    }
}
