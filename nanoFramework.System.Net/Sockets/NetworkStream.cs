using System.IO;
using System.Net;
using System.Net.Sockets;

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
        /// <exception cref="IOException"><paramref name="socket"/> is not connected. -or- The <see cref="Socket.SocketType"/> property of <paramref name="socket"/> is not <see cref="SocketType.Stream"/>. -or- <paramref name="socket"/> is in a nonblocking state.</exception>
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
        /// <exception cref="IOException"><paramref name="socket"/> is not connected. -or- The <see cref="Socket.SocketType"/> property of <paramref name="socket"/> is not <see cref="SocketType.Stream"/>. -or- <paramref name="socket"/> is in a nonblocking state.</exception>
        public NetworkStream(Socket socket, bool ownsSocket)
        {
            if (socket == null) throw new ArgumentNullException();
            
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
        /// Gets a value that indicates whether the System.Net.Sockets.NetworkStream supports reading.
        /// </summary>
        /// <value>true if data can be read from the stream; otherwise, false. The default value is true.</value>
        /// <remarks>
        /// If CanRead is true, <see cref="NetworkStream"/> allows calls to the <see cref="Read"/> method. Provide the appropriate FileAccess enumerated value in the constructor to set 
        /// the readability and write-ability of the <see cref="NetworkStream"/>. The CanRead property is set when the <see cref="NetworkStream"/> is initialized.
        /// </remarks>
        public override bool CanRead { get { return true; } }

        /// <summary>
        /// Gets a value that indicates whether the stream supports seeking. This property is not currently supported.This property always returns false.
        /// </summary>
        /// <value>false in all cases to indicate that System.Net.Sockets.NetworkStream cannot seek a specific location in the stream.</value>
        public override bool CanSeek { get { return false; } }

        /// <summary>
        /// Indicates whether timeout properties are usable for System.Net.Sockets.NetworkStream.
        /// </summary>
        /// <value>true in all cases.</value>
        public override bool CanTimeout { get { return true; } }

        /// <summary>
        /// Gets a value that indicates whether the System.Net.Sockets.NetworkStream supports writing.
        /// </summary>
        /// <value>true if data can be written to the System.Net.Sockets.NetworkStream; otherwise, false. The default value is true.</value>
        public override bool CanWrite { get { return true; } }

        /// <summary>
        /// Gets or sets the amount of time that a read operation blocks waiting for data. 
        /// </summary>
        /// <value>A Int32 that specifies the amount of time, in milliseconds, that will elapse before a read operation fails. The default value, Infinite, specifies that the read operation does not time out.</value>
        public override int ReadTimeout
        {
            get { return _socket.ReceiveTimeout; }
            set
            {   
                if (value == 0 || value < System.Threading.Timeout.Infinite) throw new ArgumentOutOfRangeException();

                _socket.ReceiveTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the amount of time that a write operation blocks waiting for data.
        /// </summary>
        /// <value>A Int32 that specifies the amount of time, in milliseconds, that will elapse before a write operation fails. The default value, Infinite, specifies that the write operation does not time out.</value>
        public override int WriteTimeout
        {
            get { return _socket.SendTimeout; }
            set
            {
                if (value == 0 || value < System.Threading.Timeout.Infinite) throw new ArgumentOutOfRangeException();

                _socket.SendTimeout = value;
            }
        }

        /// <summary>
        /// Gets the length of the data available on the stream.
        /// This property is not currently supported and always throws a NotSupportedException.
        /// </summary>
        /// <value>The length of the data available on the stream.</value>
        public override long Length
        {
            get
            {
                if (_disposed == true) throw new ObjectDisposedException();                
                if (_socket.m_Handle == -1) throw new IOException();

                return _socket.Available;
            }
        }

        /// <summary>
        /// Gets or sets the current position in the stream. This property is not currently supported and always throws a NotSupportedException.
        /// </summary>
        /// <value>The current position in the stream.</value>
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
        /// <value>true if data is available on the stream to be read; otherwise, false.</value>
        public virtual bool DataAvailable
        {
            get
            {
                if (_disposed == true) throw new ObjectDisposedException();     
                if (_socket.m_Handle == -1) throw new IOException();

                return (_socket.Available > 0);
            }
        }

        /// <summary>
        /// Closes the <see cref="NetworkStream"/> after waiting the specified time to allow data to be sent.
        /// </summary>
        /// <param name="timeout">A 32-bit signed integer that specifies the number of milliseconds to wait to send any remaining data before closing.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is less than -1.</exception>
        /// <remarks>
        /// <para>The <see cref="Close"/> method frees both unmanaged and managed resources associated with the <see cref="NetworkStream"/>. If the <see cref="NetworkStream"/> owns the underlying Socket, it is closed as well.</para>
        /// </remarks>
        public void Close(int timeout)
        {
            if (timeout < -1)
                throw new ArgumentOutOfRangeException();

            Threading.Thread.Sleep(timeout);

            Close();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="NetworkStream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        /// <remarks>
        /// <para>This method is called by the public Dispose method and the Finalize method. Dispose invokes the protected Dispose(Boolean) method with the disposing parameter set to true. Finalize invokes Dispose with disposing set to false.</para>
        /// <para>When the disposing parameter is true, this method releases all resources held by any managed objects that this NetworkStream references. This method invokes the Dispose method of each referenced object.</para>
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
                            _socket.Close();
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
        /// Reads data from the NetworkStream.
        /// </summary>
        /// <param name="buffer">An array of type <see cref="byte"/> that is the location in memory to store data read from the <see cref="NetworkStream"/>.</param>
        /// <param name="offset">The location in <paramref name="buffer"/> to begin storing the data to.</param>
        /// <param name="count">The number of bytes to read from the <see cref="NetworkStream"/>.</param>
        /// <returns>The total number of bytes read into the buffer between zero (0) and the requested count. The method returns zero (0) only if zero bytes were requested or if no more bytes are available because the peer socket performed a graceful shutdown.</returns>
        /// <exception cref="IOException">The underlying <see cref="Socket"/> is closed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="NetworkStream"/> is closed or there is a failure reading from the network.</exception>"
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is less than 0 or greater than the length of <paramref name="buffer"/>. -or- <paramref name="count"/> is less than 0 or greater than the length of <paramref name="buffer"/> minus the value of the <paramref name="offset"/> parameter.</exception>
        /// <remarks>
        /// <para>This method reads data into the <paramref name="buffer"/> parameter and returns the number of bytes successfully read. The <see cref="Read"/> operation reads as much data as is available, up to the number of bytes specified by the <paramref name="count"/> parameter. If the remote host shuts down the connection, and all available data has been received, the Read method completes immediately and return zero bytes.</para>
        /// <note type="important">
        /// Check to see if the <see cref="NetworkStream"/> is readable by calling the <see cref="CanRead"/> property. If you attempt to read from a <see cref="NetworkStream"/> that is not readable, you will get an <see cref="IOException"/> .
        /// </note>
        /// </remarks>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException();            
            if (_socket.m_Handle == -1) throw new IOException();
            if (buffer == null) throw new ArgumentNullException();
            if (offset < 0 || offset > buffer.Length) throw new ArgumentOutOfRangeException();
            if (count < 0 || count > buffer.Length - offset) throw new ArgumentOutOfRangeException();

            int available = _socket.Available;

            // we will need to read using thr timeout specified
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
        /// Sets the current position of the stream to the given value. This method is
        /// not currently supported and always throws a System.NotSupportedException.
        /// </summary>
        /// <param name="offset">This parameter is not used.</param>
        /// <param name="origin">This parameter is not used.</param>
        /// <returns>The position in the stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///  Sets the length of the stream. This method always throws a System.NotSupportedException.
        /// </summary>
        /// <param name="value">This parameter is not used.</param>
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is less than 0 or greater than the length of <paramref name="buffer"/>. -or- <paramref name="count"/> is less than 0 or greater than the length of <paramref name="buffer"/> minus the value of the <paramref name="offset"/> parameter.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="NetworkStream"/> is closed or there is a failure reading from the network.</exception>
        /// <exception cref="IOException">There was a failure while writing to the network. -or-An error occurred when accessing the socket. See the Remarks section for more information.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">The underlying <see cref="Socket"/> is not of type <see cref="SocketType.Stream"/> or <see cref="SocketType.Dgram"/>.</exception>
        /// <remarks>
        /// The Write method starts at the specified offset and sends count bytes from the contents of buffer to the network. 
        /// The Write method blocks until the requested number of bytes is sent or a <see cref="SocketException"/> is thrown. 
        /// If you receive a <see cref="SocketException"/>, use the <see cref="SocketException.ErrorCode"/> property to obtain 
        /// the specific error code, and refer to the Windows Sockets version 2 API error code documentation in MSDN for a detailed description of the error.
        /// </remarks>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed) throw new ObjectDisposedException();            
            if (_socket.m_Handle == -1) throw new IOException();
            if (buffer == null) throw new ArgumentNullException();
            if (offset < 0 || offset > buffer.Length) throw new ArgumentOutOfRangeException();
            if (count < 0 || count > buffer.Length - offset) throw new ArgumentOutOfRangeException();

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

            if (bytesSent != count) throw new IOException();
        }

        /// <inheritdoc/>
        public override int Read(SpanByte buffer)
        {
            throw new NotImplementedException();
        }
    }
}

