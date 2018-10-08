//
// Copyright (c) 2018 The nanoFramework project contributors
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

            //Do we need to try to pase this as an Address????
            string canonicalName;
            byte[][] addresses;

            NativeSocket.getaddrinfo(hostNameOrAddress, out canonicalName, out addresses);

            int cAddresses = addresses.Length;
            IPAddress[] ipAddresses = new IPAddress[cAddresses];
            IPHostEntry ipHostEntry = new IPHostEntry();

            for (int i = 0; i < cAddresses; i++)
            {
                byte[] address = addresses[i];

                SocketAddress sockAddress = new SocketAddress(address);

                AddressFamily family;

                //if(SystemInfo.IsBigEndian)
                ////{
                //    family = (AddressFamily)((address[0] << 8) | address[1]);
                //}
                //else
                {
                    family = (AddressFamily)((address[1] << 8) | address[0]);
                }
                //port address[2-3]

                if (family == AddressFamily.InterNetwork)
                {
                    //This only works with IPv4 addresses

                    uint ipAddr = (uint)((address[7] << 24) | (address[6] << 16) | (address[5] << 8) | (address[4]));

                    ipAddresses[i] = new IPAddress((long)ipAddr);
                }
            }

            ipHostEntry.hostName = canonicalName;
            ipHostEntry.addressList = ipAddresses;

            return ipHostEntry;
        }
    }
}
