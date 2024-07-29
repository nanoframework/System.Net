//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace System.Net.Security
{
    /// <summary>
    /// Defines the possible versions of <see cref="SslProtocols"/>. This enumeration supports a bitwise combination of its member values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// During the handshake, the client sends a list of SSL versions and the server chooses the best matching version from that list.
    /// </para>
    /// <para>
    /// Note: Following the full .NET implementation, .NET nanoFramework does not have any of the SSL nor Default because those are deprecated and unsecure.
    /// </para>
    /// </remarks>
    [Flags]
    public enum SslProtocols
    {
        /// <summary>
        /// Allows the operating system to choose the best protocol to use, and to block protocols that are not secure. Unless your app has a specific reason not to, you should use this value.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Specifies the TLS 1.0 security protocol. TLS 1.0 is provided for backward compatibility only. The TLS protocol is defined in IETF RFC 2246. This member is obsolete and will be removed in a future version.
        /// </summary>
        /// <remarks>
        /// If the underlying platform does not support TLS 1.0, the connection attempt will use the minimum version supported instead.
        /// </remarks>
        [Obsolete("This value is obsolete and unsecured and will be removed in a future version.")]
        Tls = 192,

        /// <summary>
        /// Specifies the TLS 1.1 security protocol. The TLS protocol is defined in IETF RFC 4346. This member is obsolete and will be removed in a future version.
        /// </summary>
        /// <remarks>
        /// If the underlying platform does not support TLS 1.1, the connection attempt will use the minimum version supported instead.
        /// </remarks>
        [Obsolete("This value is obsolete and unsecured and will be removed in a future version.")]
        Tls11 = 768,

        /// <summary>
        /// Specifies the TLS 1.2 security protocol. The TLS protocol is defined in IETF RFC 5246.
        /// </summary>
        Tls12 = 3072,

        /// <summary>
        /// Specifies the TLS 1.3 security protocol. The TLS protocol is defined in IETF RFC 8446.
        /// </summary>
        Tls13 = 12288,
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
        internal static extern int SecureServerInit(
            int sslProtocols,
            int sslCertVerify,
            X509Certificate certificate,
            X509Certificate ca,
            bool useDeviceCertificate);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern int SecureClientInit(
            int sslProtocols,
            int sslCertVerify,
            X509Certificate certificate,
            X509Certificate ca,
            bool useDeviceCertificate);

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
