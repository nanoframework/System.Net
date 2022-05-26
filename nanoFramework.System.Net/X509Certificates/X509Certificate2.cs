//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Runtime.CompilerServices;
using System.Text;

namespace System.Security.Cryptography.X509Certificates
{
    /// <summary>
    /// Represents an X.509 certificate.
    /// </summary>
    public class X509Certificate2 : X509Certificate
    {
#pragma warning disable S3459 // Unassigned members should be removed
        // field required to be accessible by native code
        private readonly byte[] _privateKey;
        private readonly string _password;
#pragma warning restore S3459 // Unassigned members should be removed

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate2"/> class.
        /// </summary>
        public X509Certificate2()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate2"/> class using information from a byte array.
        /// </summary>
        /// <param name="rawData">A byte array containing data from an X.509 certificate.</param>
        public X509Certificate2(byte[] rawData)
            : base(rawData)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate2"/> class using a string with the content of an X.509 certificate.
        /// </summary>
        /// <param name="certificate">A string containing a X.509 certificate.</param>
        /// <remarks>
        /// This methods is exclusive of .NET nanoFramework. The equivalent .NET constructor accepts a file name as the parameter.
        /// </remarks>
        public X509Certificate2(string certificate)
            : base(certificate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate2"/> class using a string with the content of an X.509 public certificate, the private key and a password used to access the private key.
        /// </summary>
        /// <param name="rawData">A string containing a X.509 certificate.</param>
        /// <param name="key">A string containing a private key in PEM or DER format.</param>
        /// <param name="password">The password required to decrypt the private key. Set to <see langword="null"/> if the <paramref name="rawData"/> or <paramref name="key"/> are not encrypted and do not require a password.</param>
        /// <remarks>
        /// This methods is exclusive of .NET nanoFramework. There is no equivalent in .NET framework.
        /// </remarks>
        public X509Certificate2(
            string rawData,
            string key,
            string password)
            : base(rawData)
        {
            var tempKey = Encoding.UTF8.GetBytes(key);

            //////////////////////////////////////////////
            // because this is parsing from a string    //
            // we need to keep the terminator           //
            //////////////////////////////////////////////
            var keyBuffer = new byte[tempKey.Length + 1];
            Array.Copy(tempKey, keyBuffer, tempKey.Length);
            keyBuffer[keyBuffer.Length - 1] = 0;

            _privateKey = keyBuffer;
            _password = password;

            DecodePrivateKeyNative(
                keyBuffer,
                password);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate2"/> class using a string with the content of an X.509 public certificate, the private key and a password used to access the certificate.
        /// </summary>
        /// <param name="rawData">A byte array containing data from an X.509 certificate.</param>
        /// <param name="key">A string containing a private key in PEM or DER format.</param>
        /// <param name="password">The password required to decrypt the private key. Set to <see langword="null"/> if the <paramref name="rawData"/> or <paramref name="key"/> are not encrypted and do not require a password.</param>
        /// <remarks>
        /// This methods is exclusive of .NET nanoFramework. There is no equivalent in .NET framework.
        /// </remarks>
        public X509Certificate2(
            byte[] rawData,
            string key,
            string password)
            : base(rawData)
        {
            var tempKey = Encoding.UTF8.GetBytes(key);

            //////////////////////////////////////////////
            // because this is parsing from a string    //
            // we need to keep the terminator           //
            //////////////////////////////////////////////
            var keyBuffer = new byte[tempKey.Length + 1];
            Array.Copy(tempKey, keyBuffer, tempKey.Length);
            keyBuffer[keyBuffer.Length - 1] = 0;

            _privateKey = keyBuffer;
            _password = password;

            DecodePrivateKeyNative(
                keyBuffer,
                password);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate2"/> class using a string with the content of an X.509 public certificate, the private key and a password used to access the certificate.
        /// </summary>
        /// <param name="rawData">A byte array containing data from an X.509 certificate.</param>
        /// <param name="key">A byte array containing a PEM private key.</param>
        /// <param name="password">The password required to decrypt the private key. <see langword="null"/> if the <paramref name="rawData"/> or <paramref name="key"/> are not encrypted.</param>
        /// <remarks>
        /// This methods is exclusive of nanoFramework. There is no equivalent in .NET framework.
        /// </remarks>
        public X509Certificate2(
            byte[] rawData,
            byte[] key,
            string password)
            : base(rawData)
        {
            _privateKey = key;
            _password = password;

            DecodePrivateKeyNative(
                key,
                password);
        }

        /// <summary>
        /// Gets a value that indicates whether an <see cref="X509Certificate2"/> object contains a private key.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="X509Certificate2"/> object contains a private key; otherwise, <see langword="false"/>.</value>
        public bool HasPrivateKey
        {
            get
            {
                return (_privateKey != null);
            }
        }

        /// <summary>
        /// Gets the private key, null if there isn't a private key.
        /// </summary>
        /// <remarks>This will give you access directly to the raw decoded byte array of the private key</remarks>
        public byte[] PrivateKey => _privateKey;

        /// <summary>
        /// Gets the public key.
        /// </summary>
        /// <remarks>This will give you access directly to the raw decoded byte array of the public key.</remarks>
        public byte[] PublicKey => RawData;

        /// <summary>
        /// Gets the date (in UTC time) after which a certificate is no longer valid.
        /// </summary>
        /// <value>A <see cref="DateTime"/> object that represents the expiration date for the certificate.</value>
        public DateTime NotAfter
        {
            get
            {
                return _expirationDate;
            }
        }

        /// <summary>
        /// Gets the date (in UTC time) on which a certificate becomes valid.
        /// </summary>
        /// <value>A <see cref="DateTime"/> object that represents the effective date of the certificate.</value>
        public DateTime NotBefore
        {
            get
            {
                return _effectiveDate;
            }
        }

        /// <summary>
        /// Gets the raw data of a certificate.
        /// </summary>
        /// <value>The raw data of the certificate as a byte array.</value>
        public byte[] RawData
        {
            get
            {
                return base.GetRawCertData();
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void DecodePrivateKeyNative(
            byte[] keyBuffer,
            string password);
    }
}
