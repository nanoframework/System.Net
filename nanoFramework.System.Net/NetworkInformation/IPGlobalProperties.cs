//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System.Runtime.CompilerServices;

namespace System.Net.NetworkInformation
{
    /// <summary>
    /// Provides information about the network connectivity of the local computer.
    /// </summary>
    public static class IPGlobalProperties
    {
        /// <summary>
        /// Gets the IP address of the network interface.
        /// </summary>
        /// <returns>An <see cref="IPAddress"/> if a valid IP address is available; otherwise, an <see cref="IPAddress.Any"/>.</returns>
        /// <remarks>
        /// This method is exclusive of .NET nanoFramework.
        /// </remarks>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static IPAddress GetIPAddress();
    }
}
