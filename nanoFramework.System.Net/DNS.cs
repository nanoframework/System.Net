//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net
{
    using System.Net.Sockets;

    /// <summary>
    /// Provides simple domain name resolution functionality.
    /// </summary>
    public static class Dns
    {
        /// <summary>
        /// Resolves a host name or IP address to an <see cref="IPHostEntry"/> instance.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <returns>An <see cref="IPHostEntry"/> instance that contains address information about the host specified in 
        /// hostNameOrAddress.
        /// </returns>
        /// <remarks>
        /// <para>The GetHostEntry method queries a DNS server for the IP address that is associated with a host name or IP address.</para>
        /// <para>When an empty string is passed as the host name, this method returns the IPv4 addresses of the local host.</para>
        /// </remarks>
        public static IPHostEntry GetHostEntry(string hostNameOrAddress)
        {
            NativeSocket.getaddrinfo(hostNameOrAddress, out string canonicalName, out byte[][] addresses);

            int addressesCount = addresses.Length;
            IPAddress[] ipAddresses = new IPAddress[addressesCount];
            IPHostEntry ipHostEntry = new();

            for (int i = 0; i < addressesCount; i++)
            {
                byte[] address = addresses[i];

                AddressFamily family = (AddressFamily)((address[1] << 8) | address[0]);

                if (family == AddressFamily.InterNetwork)
                {
                    uint ipAddr = (uint)((address[7] << 24) | (address[6] << 16) | (address[5] << 8) | (address[4]));

                    ipAddresses[i] = new IPAddress(ipAddr);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            ipHostEntry.hostName = canonicalName;
            ipHostEntry.addressList = ipAddresses;

            return ipHostEntry;
        }
    }
}
