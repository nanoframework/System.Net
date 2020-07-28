//
// Copyright (c) 2018 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace System.Net.Security
{
    /// <summary>
    /// Provides an interface to the device certificate store to manage <see cref="X509Certificate"/>.
    /// </summary>
    public static class CertificateManager
    {
        /// <summary>
        /// Adds a Certificate Authority Root bundle <see cref="X509Certificate"/> to the store.
        /// If there is already a CA Root bundle it will be replaced with this one.
        /// </summary>
        /// <param name="ca">The Certificate Authority certificate bundle to be added store.</param>
        /// <returns>
        /// True if the certificate bundle was correctly added to the device certificate store.
        /// </returns>
        /// <remarks>
        /// This method is exclusive of nanoFramework. There is no equivalent in .NET framework.
        /// </remarks>
        public static bool AddCaCertificateBundle(X509Certificate[] ca)
        {
            // build a string concatenating all the certificates
            StringBuilder bundle = new StringBuilder();

            foreach(X509Certificate cert in ca)
            {
                byte[] certRaw = cert.GetRawCertData();

                // remove the terminator from each string
                bundle.Append(Encoding.UTF8.GetString(certRaw, 0, certRaw.Length - 1));
            }

            // add terminator
            bundle.Append("\0");

            return AddCaCertificateBundle(bundle.ToString());
        }

        /// <summary>
        /// Adds a Certificate Authority Root bundle <see cref="X509Certificate"/> to the store.
        /// If there is already a CA Root bundle it will be replaced with this one.
        /// </summary>
        /// <param name="ca">The Certificate Authority certificate bundle to be added store.</param>
        /// <returns>
        /// True if the certificate bundle was correctly added to the device certificate store.
        /// </returns>
        /// <remarks>
        /// This method is exclusive of nanoFramework. There is no equivalent in .NET framework.
        /// </remarks>
        public static bool AddCaCertificateBundle(string ca)
        {
            return AddCaCertificateBundle(Encoding.UTF8.GetBytes(ca));
        }

        /// <summary>
        /// Adds a Certificate Authority Root bundle <see cref="X509Certificate"/> to the store.
        /// If there is already a CA Root bundle it will be replaced with this one.
        /// </summary>
        /// <param name="ca">The Certificate Authority certificate bundle to be added store.</param>
        /// <returns>
        /// True if the certificate bundle was correctly added to the device certificate store.
        /// </returns>
        /// <remarks>
        /// This method is exclusive of nanoFramework. There is no equivalent in .NET framework.
        /// </remarks>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool AddCaCertificateBundle(byte[] ca);
    }
}
