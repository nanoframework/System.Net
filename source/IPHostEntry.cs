//
// Copyright (c) 2018 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net
{
    /// <summary>
    /// Provides a container class for Internet host address information.
    /// </summary>
    /// <remarks>
    /// <para>The IPHostEntry class associates a Domain Name System (DNS) host name with an array of aliases and an array 
    /// of matching IP addresses.</para>
    /// <para>The IPHostEntry class is used as a helper class with the <see cref="Dns"/> class.</para>
    /// </remarks>
    public class IPHostEntry
    {
        internal string hostName;
        internal IPAddress[] addressList;

        /// <summary>
        /// Gets or sets the DNS name of the host.
        /// </summary>
        /// <value>
        /// A string that contains the primary host name for the server.
        /// </value>
        /// <remarks>
        /// The HostName property contains the primary host name for a server. If the DNS entry for the server defines additional aliases, 
        /// they will be available in the Aliases property.
        /// </remarks>
        public string HostName
        {
            get
            {
                return hostName;
            }
        }

        /// <summary>
        /// Gets or sets a list of IP addresses that are associated with a host.
        /// </summary>
        /// <value>
        /// An array of type <see cref="IPAddress"/> that contains IP addresses that resolve to the host names that are contained in the Aliases property.
        /// </value>
        public IPAddress[] AddressList
        {
            get
            {
                return addressList;
            }
        }
    } // class IPHostEntry
} // namespace System.Net


