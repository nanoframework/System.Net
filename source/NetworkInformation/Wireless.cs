////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Net.NetworkInformation
{
    /// <summary>
    /// Provides information about wireless networks based on the 802.11 standard.
    /// </summary>
    public class Wireless80211 : NetworkInterface
    {
        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum AuthenticationType
        {
            /// <summary>
            /// Selects no protocol.
            /// </summary>
            None = 0,
            /// <summary>
            /// Selects the extensible authentication protocol.
            /// </summary>
            EAP,
            /// <summary>
            /// Selects the protected extensible authentication protocol.
            /// </summary>
            PEAP,
            /// <summary>
            /// Selects the Microsoft Windows Connect Now protocol.
            /// </summary>
            WCN,
            /// <summary>
            /// Selects Open System authentication, for use with WEP encryption type.
            /// </summary>
            Open,
            /// <summary>
            /// Selects Shared Key authentication, for use with WEP encryption type.
            /// </summary>
            Shared,
        }

        /// <summary>
        /// Defines the available types of encryption for wireless networks.
        /// </summary>
        [Flags]
        public enum EncryptionType
        {
            /// <summary>
            /// Selects no encryption.
            /// </summary>
            None = 0,
            /// <summary>
            /// Selects Wired Equivalent Privacy encryption.
            /// </summary>
            WEP,
            /// <summary>
            /// Selects Wireless Protected Access encryption.
            /// </summary>
            WPA,
            /// <summary>
            /// Selects Wireless Protected Access Pre-Shared Key encryption.
            /// </summary>
            WPAPSK,
            /// <summary>
            ///  	Selects certificate encryption.
            /// </summary>
            Certificate,
        }

        /// <summary>
        /// Specifies the type of radio that the wireless network uses.
        /// </summary>
        [Flags]
        public enum RadioType
        {
            /// <summary>
            /// Selects an 802.11a-compatible radio.
            /// </summary>
            a = 1,
            /// <summary>
            /// Selects an 802.11b-compatible radio.
            /// </summary>
            b = 2,
            /// <summary>
            /// Selects an 802.11g-compatible radio.
            /// </summary>
            g = 4,
            /// <summary>
            /// Selects an 802.11n-compatible radio.
            /// </summary>
            n = 8,
        }

        private Wireless80211(int interfaceIndex)
            : base(interfaceIndex)
        {
            NetworkKey = null;
            ReKeyInternal = null;
            Id = 0xFFFFFFFF;
        }

        /// <summary>
        /// Saves the network's configuration information.
        /// </summary>
        /// <param name="wirelessConfigurations">Contains the configuration to save.</param>
        /// <param name="useEncryption">rue to use encryption; otherwise, false. </param>
        public static void SaveConfiguration(Wireless80211[] wirelessConfigurations, bool useEncryption)
        {
            // Before we update validate whether settings conform to right characteristics.
            for (int i = 0; i < wirelessConfigurations.Length; i++)
            {
                ValidateConfiguration(wirelessConfigurations[i]);
            }

            for (int i = 0; i < wirelessConfigurations.Length; i++)
            {
                UpdateConfiguration(wirelessConfigurations[i], useEncryption);
            }

            SaveAllConfigurations();
        }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        /// <param name="wirelessConfiguration"></param>
        public static void ValidateConfiguration(Wireless80211 wirelessConfiguration)
        {
            if ((wirelessConfiguration.Authentication < AuthenticationType.None  ) || 
                (wirelessConfiguration.Authentication > AuthenticationType.Shared) ||
                 (wirelessConfiguration.Encryption < EncryptionType.None         ) || 
                 (wirelessConfiguration.Encryption > EncryptionType.Certificate  ) ||
                 (wirelessConfiguration.Radio < RadioType.a                      ) || 
                 (wirelessConfiguration.Radio > (RadioType.n | RadioType.g | RadioType.b | RadioType.a)))
            {
                throw new ArgumentOutOfRangeException();
            }

            if ((wirelessConfiguration.PassPhrase    == null) || 
                (wirelessConfiguration.NetworkKey    == null) || 
                (wirelessConfiguration.ReKeyInternal == null) || 
                (wirelessConfiguration.Ssid          == null))
            {
                throw new ArgumentNullException();
            }

            if ((wirelessConfiguration.PassPhrase.Length    >= MaxPassPhraseLength) || 
                (wirelessConfiguration.NetworkKey.Length    >  NetworkKeyLength   ) || 
                (wirelessConfiguration.ReKeyInternal.Length >  ReKeyInternalLength) || 
                (wirelessConfiguration.Ssid.Length          >= SsidLength         ))
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Specifies the type of authentication used on the wireless network.
        /// </summary>
        public AuthenticationType Authentication;
        /// <summary>
        /// Specifies the type of encryption used on the wireless network.
        /// </summary>
        public EncryptionType Encryption;
        /// <summary>
        /// Specifies the type of radio used by the wireless network adapter.
        /// </summary>
        public RadioType Radio;
        /// <summary>
        /// Contains the network passphrase.
        /// </summary>
        public string PassPhrase;
        /// <summary>
        /// Contains the network key.
        /// </summary>
        public byte[] NetworkKey;
        /// <summary>
        /// Contains the key used when rekeying the network.
        /// </summary>
        public byte[] ReKeyInternal;
        /// <summary>
        /// Contains the network's SSID.
        /// </summary>
        public string Ssid;
        /// <summary>
        /// Contains the ID of the wireless network.
        /// </summary>
        public readonly uint Id;

        /// <summary>
        /// Contains the network key length. The value is set to 256.
        /// </summary>
        public const int NetworkKeyLength = 256;
        /// <summary>
        /// Contains the ReKey internal length. The value is set to 32.
        /// </summary>
        public const int ReKeyInternalLength = 32;
        /// <summary>
        /// Contains the Ssid length. The value is set to 32.
        /// </summary>
        public const int SsidLength = 32;
        /// <summary>
        /// Contains the maximum pass phrase length. The value is set to 64.
        /// </summary>
        public const int MaxPassPhraseLength = 64;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static void UpdateConfiguration(Wireless80211 wirelessConfigurations, bool useEncryption);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static void SaveAllConfigurations();
    }
}


