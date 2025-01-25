//
// Copyright (c) .NET Foundation and Contributors
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

#pragma warning disable S1144 // field used in native code
        /// <summary>
        /// This is the configuration index as provided by the device storage manager.
        /// </summary>
        private readonly int _configurationIndex;
#pragma warning restore S1144 // Unused private types or members should be removed

        private readonly uint _id;
        private AuthenticationType _authentication;
        private EncryptionType _encryption;
        private RadioType _radio;
        private string _password;
        private string _ssid;
        private ConfigurationOptions _options;

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
        /// Contains flags for the Wireless connection 
        /// </summary>
        public ConfigurationOptions Options { get => _options; set => _options = value; }

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

#pragma warning disable IDE0032 // nanoFramework doesn't support auto-properties

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
#pragma warning disable S3928 // OK to not include a meaningful message
                throw new ArgumentNullException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
            }

            // check password and SSID length
            if ((_password.Length    > MaxPasswordLength) || 
                (_ssid.Length        > MaxSsidLength))
            {
#pragma warning disable S3928 // OK to not include a meaningful message
                throw new ArgumentOutOfRangeException();
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
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

        /// <summary>
        /// Configuration flags used for Wireless configuration.
        /// </summary>
        [Flags]
        public enum ConfigurationOptions : byte
        {
            /// <summary>
            /// No option set.
            /// </summary>
            None = 0,

            /// <summary>
            /// Disables the Wireless station.
            /// </summary>
            Disable = 0x01,

            /// <summary>
            /// Enables the Wireless station.
            /// If not set the wireless station is disabled.
            /// </summary>
            Enable = 0x02,

            /// <summary>
            /// Will auto connect when AP is available or after being disconnected.
            /// This option forces enabling the Wireless station.
            /// </summary>
            AutoConnect = 0x04 | Enable,

            /// <summary>
            /// Enables SmartConfig (if available) for this Wireless station.
            /// This option forces enabling the Wireless station.
            /// </summary>
            SmartConfig = 0x08 | Enable,
        };

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
