//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Sockets
{


    /// <summary>
    /// Defines error codes returned by native SSL initialisation.
    /// </summary>
    /// <remarks>
    /// Values are kept in sync with the <c>SSL_Error</c> enum in the native interpreter
    /// (<c>ssl_functions.h</c>). <see cref="System.Net.Security.SslStream"/> surfaces these
    /// through <see cref="System.Security.Cryptography.CryptographicException.ErrorCode"/>
    /// when SSL context setup fails.
    /// </remarks>
    ///
    public enum SslError : byte
    {
        /// <summary>No error; SSL context initialisation succeeded.</summary>
        None = 0,

        /// <summary>
        /// All SSL context slots are in use.
        /// Close an existing SSL connection before opening a new one.
        /// </summary>
        NoFreeContext,

        /// <summary>
        /// A memory allocation failed while setting up the SSL context.
        /// The device may be low on heap.
        /// </summary>
        OutOfMemory,

        /// <summary>
        /// The DRBG (Deterministic Random Bit Generator) seed step failed.
        /// The entropy source could not be initialised.
        /// </summary>
        DrbgSeedFailed,

        /// <summary>
        /// Setting TLS configuration defaults failed.
        /// This is an internal mbedTLS error that should not occur under normal conditions.
        /// </summary>
        ConfigDefaultsFailed,

        /// <summary>
        /// The requested TLS protocol version is not supported on this device.
        /// Use a protocol version that the target hardware supports.
        /// </summary>
        UnsupportedProtocolVersion,

        /// <summary>
        /// The supplied private key could not be parsed.
        /// Verify the key is in a supported format (PEM or DER) and is not corrupted.
        /// </summary>
        PrivateKeyParseFailed,

        /// <summary>
        /// The supplied certificate could not be parsed.
        /// Verify the certificate is in a supported format (PEM or DER) and is not corrupted.
        /// </summary>
        CertificateParseFailed,

        /// <summary>
        /// Configuring the own certificate and private key pair on the SSL context failed.
        /// This is an internal mbedTLS error that should not occur under normal conditions.
        /// </summary>
        OwnCertConfigFailed,

        /// <summary>
        /// Final SSL context setup failed.
        /// This is an internal mbedTLS error that should not occur under normal conditions.
        /// </summary>
        SetupFailed,
    }
}
