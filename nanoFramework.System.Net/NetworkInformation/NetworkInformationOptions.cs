//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.NetworkInformation
{
    /// <summary>
    /// Options for <see cref="NetworkInformation"/>.
    /// </summary>
    internal enum NetworkInformationOptions
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
}
