// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace System.Net.Sockets
{
    /// <summary>
    /// Provides the underlying stream of data for network access.
    /// </summary>
    public class NetworkStream : Stream
    {

        // Summary:
        // Internal members

        // Internal Socket object
        internal Socket _socket;

        /// <summary>
        /// Internal property used to store the socket type
        /// </summary>
        protected int _socketType;

        /// <summary>
        /// Internal endpoint ref used for dgram sockets
        /// </summary>
        protected EndPoint _remoteEndPoint;

        // Internal flags
        private bool _ownsSocket;

        /// <summary>
        /// Internal disposed flag
        /// </summary>
        protected bool _disposed;

        /// <summary>
        /// Creates a new instance of the <see cref="NetworkStream"/> class for the specified <see cref="Socket"/>.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> that the <see cref="NetworkStream"/> will use to send and receive data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="socket"/> is <see langword="null"/>.</exception>
        /// <exception cref="IOException">The <paramref name="socket"/> is not connected or the remote endpoint is not available.</exception>
        public NetworkStream(Socket socket)
            : this(socket, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkStream"/> class for the specified
        /// <see cref="Socket"/> with the specified <see cref="Socket"/> ownership.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> that the <see cref="NetworkStream"/> will
        /// use to send and receive data.</param>
        /// <param name="ownsSocket"><see langword="true"/> to indicate that the <see cref="NetworkStream"/> will take ownership of the <see cref="Socket"/>;
        /// otherwise, <see langword="false"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="socket"/> is <see langword="null"/>.</exception>
        /// <exception cref="IOException">The <paramref name="socket"/> is not connected or the remote endpoint is not available.</exception>
        public NetworkStream(Socket socket, bool ownsSocket)
        {
            ArgumentNullException.ThrowIfNull(socket);

            // This should throw a SocketException if not connected
            try
            {
                _remoteEndPoint = socket.RemoteEndPoint;
            }
            catch (Exception e)
            {
                int errCode = (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Error);

                throw new IOException(errCode.ToString(), e);
            }

            // Set the internal socket
            _socket = socket;

            // set the socket type
            _socketType = (int)socket.SocketType;

            _ownsSocket = ownsSocket;
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="NetworkStream"/> supports reading.
        /// </summary>
        /// <value><see langword="true"/> if data can be read from the stream; otherwise, <see langword="false"/>.</value>
        public override bool CanRead { get { return true; } }

        /// <summary>
        /// Gets a value that indicates whether the stream supports seeking.
        /// </summary>
        /// <value><see langword="false"/> in all cases to indicate that <see cref="NetworkStream"/> cannot seek a specific location in the stream.</value>
        public override bool CanSeek { get { return false; } }

        /// <summary>
        /// Indicates whether timeout properties are usable for <see cref="NetworkStream"/>.
        /// </summary>
        /// <value><see langword="true"/> in all cases.</value>
        public override bool CanTimeout { get { return true; } }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="NetworkStream"/> supports writing.
        /// </summary>
        /// <value><see langword="true"/> if data can be written to the <see cref="NetworkStream"/>; otherwise, <see langword="false"/>.</value>
        public override bool CanWrite { get { return true; } }

        /// <summary>
        /// Gets or sets the amount of time that a read operation blocks waiting for data.
        /// </summary>
        /// <value>An <see cref="int"/> that specifies the amount of time, in milliseconds, that will elapse before a read operation fails. The default value, <see cref="Threading.Timeout.Infinite"/>, specifies that the read operation does not time out.</value>
        /// <exception cref="ArgumentOutOfRangeException">The value is0 or less than <see cref="Threading.Timeout.Infinite"/>.</exception>
        public override int ReadTimeout
        {
            get { return _socket.ReceiveTimeout; }
            set
            {
                if (value is 0 or < Threading.Timeout.Infinite)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _socket.ReceiveTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of time that a write operation blocks waiting for data.
        /// </summary>
        /// <value>An <see cref="int"/> that specifies the amount of time, in milliseconds, that will elapse before a write operation fails. The default value, <see cref="Threading.Timeout.Infinite"/>, specifies that the write operation does not time out.</value>
        /// <exception cref="ArgumentOutOfRangeException">The value is0 or less than <see cref="Threading.Timeout.Infinite"/>.</exception>
        public override int WriteTimeout
        {
            get { return _socket.SendTimeout; }
            set
            {
                if (value is 0 or < Threading.Timeout.Infinite)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _socket.SendTimeout = value;
            }
        }

        /// <summary>
        /// Gets the amount of data available on the stream to be read.
        /// </summary>
        /// <value>The amount of data that has been received from the network and is available to be read.</value>
        /// <exception cref="ObjectDisposedException">The <see cref="NetworkStream"/> has been disposed.</exception>
        /// <exception cref="IOException">The underlying <see cref="Socket"/> is closed.</exception>
        /// <remarks>
        /// This value corresponds to the underlying socket's <see cref="Socket.Available"/> property.
        /// </remarks>
        public override long Length
        {
            get
            {
                if (_disposed == true)
                {
                    throw new ObjectDisposedException();
                }

                if (_socket.m_Handle == -1)
                {
                    throw new IOException();
                }

                return _socket.Available;
            }
        }

        /// <summary>
        /// Gets or sets the current position in the stream.
        /// </summary>
        /// <value>The current position in the stream.</value>
        /// <exception cref="NotSupportedException">Getting or setting the position is not supported for <see cref="NetworkStream"/>.</exception>
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets a value that indicates whether data is available on the <see cref="NetworkStream"/> to be read.
        /// </summary>
        /// <value><see langword="true"/> if data is available on the stream to be read; otherwise, <see langword="false"/>.</value>
        /// <exception cref="ObjectDisposedException">The <see cref="NetworkStream"/> has been disposed.</exception>
        /// <exception cref="IOException">The underlying <see cref="Socket"/> is closed.</exception>
        public virtual bool DataAvailable
        {
            get
            {
                if (_disposed == true)
                {
                    throw new ObjectDisposedException();
                }

                if (_socket.m_Handle == -1)
                {
                    throw new IOException();
                }

                return _socket.Available > 0;
            }
        }

        /// <summary>
        /// Closes the <see cref="NetworkStream"/> after waiting the specified time to allow data to be sent.
        /// </summary>
        /// <param name="timeout">A32-bit signed integer that specifies the number of milliseconds to wait to send any remaining data before closing.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is less than -1.</exception>
        /// <remarks>
        /// <para>The <see cref="Close"/> method frees both unmanaged and managed resources associated with the <see cref="NetworkStream"/>. If the <see cref="NetworkStream"/> owns the underlying <see cref="Socket"/>, it is closed as well.</para>
        /// </remarks>
        public void Close(int timeout)
        {
            if (timeout < -1)
            {
                throw new ArgumentOutOfRangeException();
            }

            Threading.Thread.Sleep(timeout);

            Close();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="NetworkStream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        /// <remarks>
        /// <para>This method is called by the public <see cref="Dispose(bool)"/> method and the Finalize method. <see cref="Dispose(bool)"/> invokes the protected <see cref="Dispose(bool)"/> method with the <paramref name="disposing"/> parameter set to <see langword="true"/>. Finalize invokes <see cref="Dispose(bool)"/> with <paramref name="disposing"/> set to <see langword="false"/>.</para>
        /// <para>When the <paramref name="disposing"/> parameter is <see langword="true"/>, this method releases all resources held by any managed objects that this <see cref="NetworkStream"/> references. This method invokes the <see cref="Dispose(bool)"/> method of each referenced object.</para>
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (_ownsSocket == true)
                        {
                            _socket.Close();
                        }
                    }
                }
                finally
                {
                    _disposed = true;
                }
            }
        }

        /// <summary>
        /// Flushes data from the stream. This method is reserved for future use.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Reads data from the <see cref="NetworkStream"/>.
        /// </summary>
        /// <param name="buffer">An array of type <see cref="byte"/> that is the location in memory to store data read from the <see cref="NetworkStream"/>.</param>
        /// <param name="offset">The location in <paramref name="buffer"/> to begin storing the data.</param>
        /// <param name="count">The number of bytes to read from the <see cref="NetworkStream"/>.</param>
        /// <returns>The number of bytes read from the <see cref="NetworkStream"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is less than0 or greater than the length of <paramref name="buffer"/>; or <paramref name="count"/> is less than0 or greater than the length of <paramref name="buffer"/> minus <paramref name="offset"/>.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="NetworkStream"/> is closed.</exception>
        /// <exception cref="IOException">The underlying <see cref="Socket"/> is closed, or an error occurred when accessing the socket.</exception>
        /// <remarks>
        /// <para>This method reads data into the <paramref name="buffer"/> parameter and returns the number of bytes successfully read. If no data is available for reading, the <see cref="Read(byte[], int, int)"/> method returns0. The read operation reads as much data as is available, up to the number of bytes specified by the <paramref name="count"/> parameter.</para>
        /// <para>If the remote host shuts down the connection, and all available data has been received, the <see cref="Read(byte[], int, int)"/> method completes immediately and returns zero bytes.</para>
        /// </remarks>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (count < 0 || count > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }

            return ReadCore(buffer, offset, count);
        }

        /// <summary>
        /// Reads data from the <see cref="NetworkStream"/> and stores it to a span of bytes in memory.
        /// </summary>
        /// <param name="buffer">A region of memory to store data read from the <see cref="NetworkStream"/>.</param>
        /// <returns>The total number of bytes read into the buffer, between zero (0) and the length of the buffer. The method returns zero (0) only if zero bytes were requested or if no more bytes are available because the peer socket performed a graceful shutdown.</returns>
        /// <exception cref="ObjectDisposedException">The <see cref="NetworkStream"/> is closed.</exception>
        /// <exception cref="IOException">
        /// <para>
        /// An error occurred when accessing the socket.
        /// 
        /// -or-
        /// </para>
        /// <para>
        /// There is a failure reading from the network.
        /// </para>
        /// </exception>
        /// <remarks>
        /// This method reads as much data as is available into the <paramref name="buffer"/> parameter and returns the number of bytes successfully read.
        /// </remarks>
        public override int Read(Span<byte> buffer)
        {
            // developer note: need to convert span to byte array for nanoFramework compatibility
            // in a future iteration we should have a native method accepting spans directly
            byte[] array = new byte[buffer.Length];
            int bytesRead = ReadCore(array, 0, array.Length);

            // copy the data back to the span
            for (int i = 0; i < bytesRead; i++)
            {
                buffer[i] = array[i];
            }

            return bytesRead;
        }

        /// <summary>
        /// Core read implementation that validates state and receives data through the socket.
        /// </summary>
        /// <param name="buffer">The buffer to store data read from the stream.</param>
        /// <param name="offset">The offset in the buffer to start storing data.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes read from the stream.</returns>
        private int ReadCore(byte[] buffer, int offset, int count)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (_socket.m_Handle == -1)
            {
                throw new IOException();
            }

            int available = _socket.Available;

            // we will need to read using the timeout specified
            // if there is data available we can return with that data only
            // the underlying socket infrastructure will handle the timeout
            if (count > available && available > 0)
            {
                count = available;
            }

            if (_socketType == (int)SocketType.Stream)
            {
                return _socket.Receive(buffer, offset, count, SocketFlags.None);
            }
            else if (_socketType == (int)SocketType.Dgram)
            {
                return _socket.ReceiveFrom(buffer, offset, count, SocketFlags.None, ref _remoteEndPoint);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Sets the current position of the stream to the given value. This method is not supported.
        /// </summary>
        /// <param name="offset">This parameter is not used.</param>
        /// <param name="origin">This parameter is not used.</param>
        /// <returns>This method does not return. It always throws a <see cref="NotSupportedException"/>.</returns>
        /// <exception cref="NotSupportedException">Seeking is not supported for <see cref="NetworkStream"/>.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the length of the stream. This method is not supported.
        /// </summary>
        /// <param name="value">This parameter is not used.</param>
        /// <exception cref="NotSupportedException">Setting the length is not supported for <see cref="NetworkStream"/>.</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes data to the <see cref="NetworkStream"/>.
        /// </summary>
        /// <param name="buffer">An array of type <see cref="byte"/> that contains the data to write to the <see cref="NetworkStream"/>.</param>
        /// <param name="offset">The location in <paramref name="buffer"/> from which to start writing data.</param>
        /// <param name="count">The number of bytes to write to the <see cref="NetworkStream"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is less than0 or greater than the length of <paramref name="buffer"/>; or <paramref name="count"/> is less than0 or greater than the length of <paramref name="buffer"/> minus <paramref name="offset"/>.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="NetworkStream"/> is closed.</exception>
        /// <exception cref="IOException">There was a failure while writing to the network, or the underlying <see cref="Socket"/> is closed.</exception>
        /// <remarks>
        /// The <see cref="Write(byte[], int, int)"/> method starts at the specified <paramref name="offset"/> and sends <paramref name="count"/> bytes from the contents of <paramref name="buffer"/> to the network.
        /// The <see cref="Write(byte[], int, int)"/> method blocks until the requested number of bytes is sent or a <see cref="SocketException"/> is thrown.
        /// </remarks>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (count < 0 || count > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }

            WriteCore(buffer, offset, count);
        }

        /// <summary>
        /// Writes data to the <see cref="NetworkStream"/>.
        /// </summary>
        /// <param name="buffer">A region of memory that contains the data to write to the <see cref="NetworkStream"/>.</param>
        /// <exception cref="ObjectDisposedException">The <see cref="NetworkStream"/> is closed.</exception>
        /// <exception cref="IOException">There was a failure while writing to the network, or the underlying <see cref="Socket"/> is closed.</exception>
        /// <remarks>
        /// The <see cref="Write(ReadOnlySpan{byte})"/> method sends the bytes from the <paramref name="buffer"/> to the network.
        /// The <see cref="Write(ReadOnlySpan{byte})"/> method blocks until the requested number of bytes is sent or a <see cref="SocketException"/> is thrown.
        /// </remarks>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            // Convert span to byte array for nanoFramework compatibility
            byte[] array = buffer.ToArray();
            WriteCore(array, 0, array.Length);
        }

        /// <summary>
        /// Core write implementation that validates state and sends data through the socket.
        /// </summary>
        /// <param name="buffer">The buffer containing data to write.</param>
        /// <param name="offset">The offset in the buffer to start writing from.</param>
        /// <param name="count">The number of bytes to write.</param>
        private void WriteCore(byte[] buffer, int offset, int count)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException();
            }

            if (_socket.m_Handle == -1)
            {
                throw new IOException();
            }

            int bytesSent = 0;

            if (_socketType == (int)SocketType.Stream)
            {
                bytesSent = _socket.Send(buffer, offset, count, SocketFlags.None);
            }
            else if (_socketType == (int)SocketType.Dgram)
            {
                bytesSent = _socket.SendTo(buffer, offset, count, SocketFlags.None, _socket.RemoteEndPoint);
            }
            else
            {
                throw new NotSupportedException();
            }

            if (0 != count)
            {
                throw new IOException();
            }
        }
    }
}
