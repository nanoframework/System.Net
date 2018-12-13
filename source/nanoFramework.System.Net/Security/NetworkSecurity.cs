//
// Copyright (c) 2018 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace System.Net.Security
{
    /// <summary>
    /// Defines the possible versions of Secure Sockets Layer (SSL).
    /// </summary>
    [FlagsAttribute]
    public enum SslProtocols
    {
        /// <summary>
        /// No SSL protocol is specified.
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Specifies the SSL 3.0 protocol. 
        /// SSL 3.0 has been superseded by the TLS protocol and is provided for backward compatibility only.
        /// </summary>
        SSLv3 = 0x08,
        /// <summary>
        /// Specifies the TLS 1.0 security protocol. The TLS protocol is defined in IETF RFC 2246.
        /// </summary>
        TLSv1 = 0x10,
        /// <summary>
        /// Specifies the TLS 1.1 security protocol. The TLS protocol is defined in IETF RFC 4346.
        /// </summary>
        TLSv11 = 0x20,
        /// <summary>
        /// Specifies the TLS 1.2 security protocol. The TLS protocol is defined in IETF RFC 5246.
        /// </summary>
        TLSv12 = 0x40,
        /// <summary>
        /// Specifies that either Secure Sockets Layer (SSL) 3.0 or Transport Layer Security (TLS) 1.0 are acceptable for secure communications.
        /// </summary>
        Default = SSLv3 | TLSv1,
    }

    /// <summary>
    /// The verification scheme to use for authentication.
    /// </summary>
    public enum SslVerification
    {
        /// <summary>
        /// No verification of certificates is required for authentication.
        /// </summary>
        NoVerification = 1,
        /// <summary>
        /// If authenticating as a client, verifies the peer certificate and fails if no certificate is sent. If authenticating as a server, 
        /// it verifies the peer certificate only if a certificate is sent.
        /// </summary>
        VerifyPeer = 2,
        /// <summary>
        /// A certificate is required for authentication. If authenticating as a client, the server certificate is required. 
        /// If authenticating as a server, the client certificate is required.
        /// </summary>
        CertificateRequired = 4,
        /// <summary>
        /// Verify the client certificate only once. Applies only to authenticating as a server.
        /// </summary>
        VerifyClientOnce = 8,
    }

    internal static class SslNative
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int SecureServerInit(int sslProtocols, int sslCertVerify, X509Certificate certificate, X509Certificate ca);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int SecureClientInit(int sslProtocols, int sslCertVerify, X509Certificate certificate, X509Certificate ca);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void UpdateCertificates(int contextHandle, X509Certificate certificate, X509Certificate[] ca);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void SecureAccept(int contextHandle, object socket);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void SecureConnect(int contextHandle, string targetHost, object socket);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int SecureRead(object socket, byte[] buffer, int offset, int size, int timeout_ms);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int SecureWrite(object socket, byte[] buffer, int offset, int size, int timeout_ms);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int SecureCloseSocket(object socket);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int ExitSecureContext(int contextHandle);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int DataAvailable(object socket);
    }
}
