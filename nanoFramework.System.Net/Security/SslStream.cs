//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace System.Net.Security
{
    /// <summary>
    /// Provides a stream used for client-server communication that uses the Secure Socket Layer (SSL) security 
    /// protocol to authenticate the server and optionally the client.
    /// </summary>
    public class SslStream : NetworkStream
    {
        private SslVerification _sslVerification;

        // Internal flags
        private int _sslContext;
        private bool _isServer;

        /// <summary>
        /// Option for SSL verification.
        /// The default behaviour is <see cref="SslVerification.CertificateRequired"/>.
        /// </summary>
        public SslVerification SslVerification { get => _sslVerification; set => _sslVerification = value; }

        //--//

        /// <summary>
        /// Initializes a new instance of the SslStream class using the specified Socket.
        /// </summary>
        /// <param name="socket">A valid socket that currently has a TCP connection.</param>
        /// <remarks>
        /// The SslStream maintains the lifetime of the socket. When the SslStream object is disposed, 
        /// the underlying TCP socket will be closed.
        /// </remarks>
        public SslStream(Socket socket)
            : base(socket, false)
        {
            if (SocketType.Stream != (SocketType)_socketType)
            {
                throw new NotSupportedException();
            }

            _sslContext = -1;
            _isServer = false;

            _sslVerification = SslVerification.CertificateRequired;
        }

        /// <summary>
        /// Called by clients to authenticate the server and optionally the client in a client-server connection. 
        /// The authentication process uses the specified SSL protocols.
        /// </summary>
        /// <param name="targetHost">The name of the server that will share this SslStream.</param>
        /// <param name="enabledSslProtocols">The <see cref="SslProtocols"/> value that represents the protocol used for authentication.</param>
        public void AuthenticateAsClient(string targetHost, SslProtocols enabledSslProtocols)
        {
            Authenticate(false, targetHost, null, null, enabledSslProtocols);
        }

        /// <summary>
        /// Called by clients to authenticate the server and optionally the client in a client-server connection. 
        /// The authentication process uses the specified certificate collections and SSL protocols.
        /// </summary>
        /// <param name="targetHost">The name of the server that will share this SslStream.</param>
        /// <param name="clientCertificate">The client certificate.</param>
        /// <param name="enabledSslProtocols">The <see cref="SslProtocols"/> value that represents the protocol used for authentication.</param>
        public void AuthenticateAsClient(string targetHost, X509Certificate clientCertificate, SslProtocols enabledSslProtocols)
        {
            Authenticate(false, targetHost, clientCertificate, null, enabledSslProtocols);
        }

        /// <summary>
        /// Called by clients to authenticate the server and optionally the client in a client-server connection. 
        /// The authentication process uses the specified certificate collections and SSL protocols.
        /// </summary>
        /// <param name="targetHost">The name of the server that will share this SslStream.</param>
        /// <param name="clientCertificate">The client certificate.</param>
        /// <param name="ca">Certificate Authority certificate to use for authentication with the server.</param>
        /// <param name="enabledSslProtocols">The <see cref="SslProtocols"/> value that represents the protocol used for authentication.</param>
        public void AuthenticateAsClient(string targetHost, X509Certificate clientCertificate, X509Certificate ca, SslProtocols enabledSslProtocols)
        {
            Authenticate(false, targetHost, clientCertificate, ca, enabledSslProtocols);
        }

        /// <summary>
        /// Called by servers to authenticate the server and optionally the client in a client-server connection using the specified certificate, 
        /// verification requirements and security protocol.
        /// </summary>
        /// <param name="serverCertificate">The certificate used to authenticate the server.</param>
        /// <param name="enabledSslProtocols">The protocols that may be used for authentication.</param>
        public void AuthenticateAsServer(X509Certificate serverCertificate, SslProtocols enabledSslProtocols)
        {
            Authenticate(true, "", serverCertificate, null, enabledSslProtocols);
        }

        /// <summary>
        /// Called by servers to authenticate the server and optionally the client in a client-server connection using the specified certificates, requirements and security protocol.
        /// </summary>
        /// <param name="serverCertificate">The X509Certificate used to authenticate the server.</param>
        /// <param name="clientCertificateRequired">A <see cref="Boolean"/> value that specifies whether the client is asked for a certificate for authentication. Note that this is only a request, if no certificate is provided, the server still accepts the connection request.</param>
        /// <param name="enabledSslProtocols">The protocols that may be used for authentication.</param>
        public void AuthenticateAsServer(X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols enabledSslProtocols)
        {
            SslVerification = clientCertificateRequired ? SslVerification.VerifyClientOnce : SslVerification.NoVerification;

            Authenticate(true, "", serverCertificate, null, enabledSslProtocols);
        }

        internal void Authenticate(bool isServer, string targetHost, X509Certificate certificate, X509Certificate ca, SslProtocols enabledSslProtocols)
        {
            if (-1 != _sslContext) throw new InvalidOperationException();

            _isServer = isServer;

            try
            {
                if (isServer)
                {
                    _sslContext = SslNative.SecureServerInit((int)enabledSslProtocols, (int)_sslVerification, certificate, ca);
                    SslNative.SecureAccept(_sslContext, _socket);
                }
                else
                {
                    _sslContext = SslNative.SecureClientInit((int)enabledSslProtocols, (int)_sslVerification, certificate, ca);
                    SslNative.SecureConnect(_sslContext, targetHost, _socket);
                }
            }
            catch
            {
                if (_sslContext != -1)
                {
                    SslNative.ExitSecureContext(_sslContext);
                    _sslContext = -1;
                }

                throw;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the local side of the connection used by this SslStream was authenticated as the server.
        /// </summary>
        public bool IsServer { get { return _isServer; } }

        /// <summary>
        /// Gets the length of the stream. (Overrides NetworkStream. . :: . .Length.)
        /// </summary>
        public override long Length
        {
            get
            {
                if (_disposed == true) throw new ObjectDisposedException();
                if (_socket == null) throw new IOException();

                return SslNative.DataAvailable(_socket);
            }
        }

        /// <summary>
        /// Gets a value the indicates whether data is available in the stream. (Overrides NetworkStream. . :: . .DataAvailable.)
        /// </summary>
        public override bool DataAvailable
        {
            get
            {
                if (_disposed == true) throw new ObjectDisposedException();
                if (_socket == null) throw new IOException();

                return (SslNative.DataAvailable(_socket) > 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ~SslStream()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the SslStream and optionally releases the managed resources. 
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                if(_socket.m_Handle != -1)
                {
                    SslNative.SecureCloseSocket(_socket);
                    _socket.m_Handle = -1;
                }

                if (_sslContext != -1) 
                {
                    SslNative.ExitSecureContext(_sslContext);
                    _sslContext = -1;
                }
            }
        }

        /// <summary>
        /// Reads data from this stream and stores it in the specified array.
        /// </summary>
        /// <param name="buffer">An array that receives the bytes read from this stream.</param>
        /// <param name="offset">An integer that contains the zero-based location in buffer at which to begin storing the data read from this stream.</param>
        /// <param name="size">The maximum number of bytes to read from this stream.</param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (size < 0 || size > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }

            return SslNative.SecureRead(_socket, buffer, offset, size, _socket.ReceiveTimeout);
        }

        /// <summary>
        /// Write the specified number of bytes to the underlying stream using the specified buffer and offset.
        /// </summary>
        /// <param name="buffer">An array that supplies the bytes written to the stream.</param>
        /// <param name="offset">he zero-based location in buffer at which to begin reading bytes to be written to the stream.</param>
        /// <param name="size">The number of bytes to read from buffer.</param>
        public override void Write(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (size < 0 || size > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }

            SslNative.SecureWrite(_socket, buffer, offset, size, _socket.SendTimeout);
        }
    }
}
