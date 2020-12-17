//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Sockets
{
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Diagnostics;


    /// <summary>
    /// Implements the Berkeley sockets interface.
    /// </summary>
    public class Socket : IDisposable
    {
        /* WARNING!!!!
         * The m_Handle field MUST be the first field in the Socket class; it is expected by
         * the SPOT.NET.SocketNative class.
         */
        //     [FieldNoReflection]
        internal int m_Handle = -1;

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private bool m_fBlocking = true;

        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private EndPoint m_localEndPoint = null;

        // timeout values are stored in uSecs since the Poll method requires it.
        private int m_recvTimeout = Timeout.Infinite;
        private int m_sendTimeout = Timeout.Infinite;

        // socket type
        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private SocketType _socketType = SocketType.Unknown;

        // Our internal state doesn't automatically get updated after a non-blocking connect
        // completes.  Keep track of whether we're doing a non-blocking connect, and make sure
        // to poll for the real state until we're done connecting.
        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private bool _nonBlockingConnectInProgress;

        // Keep track of the kind of endpoint used to do a non-blocking connect, so we can set
        // it to m_RightEndPoint when we discover we're connected.
        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        private EndPoint _nonBlockingConnectRightEndPoint;

        // _rightEndPoint is null if the socket has not been bound. Otherwise, it is any EndPoint of the correct type (IPEndPoint, etc).
        [Diagnostics.DebuggerBrowsable(Diagnostics.DebuggerBrowsableState.Never)]
        internal EndPoint _rightEndPoint;

        /// <summary>
        /// Initializes a new instance of the Socket class using the specified address family, socket type and protocol.
        /// </summary>
        /// <param name="addressFamily">One of the <see cref="AddressFamily"/> values.</param>
        /// <param name="socketType">One of the <see cref="SocketType"/> values.</param>
        /// <param name="protocolType">One of the <see cref="ProtocolType"/> values.</param>
        /// <remarks>
        /// The addressFamily parameter specifies the addressing scheme that the <see cref="Socket"/> class uses, the socketType parameter specifies the type of the <see cref="Socket"/> class, 
        /// and the protocolType parameter specifies the protocol used by <see cref="Socket"/>. The three parameters are not independent. Some address families restrict which 
        /// protocols can be used with them, and often the <see cref="Socket"/> type is implicit in the protocol. If the combination of address family, <see cref="Socket"/> type, and protocol type
        /// esults in an invalid Socket, this constructor throws a SocketException.
        /// </remarks>
        public Socket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            // Unhandled exceptions were causing nanoFramework applications to crash
            try
            {
                m_Handle = NativeSocket.socket((int)addressFamily, (int)socketType, (int)protocolType);
                _socketType = socketType;
            }
            catch (SocketException e)
            {
                throw e;
            }
        }

        private Socket(int handle)
        {
            m_Handle = handle;
        }

        /// <summary>
        /// Gets the amount of data that has been received from the network and is available to be read.
        /// </summary>
        /// <value>
        /// An integer error code that is associated with this exception.
        /// </value>
        /// <remarks>
        /// <para>
        /// If you are using a non-blocking <see cref="Socket"/>, <see cref="Available"/> is a good way to determine whether data is queued for reading, before calling <see cref="Receive(byte[])"/>. The available data
        /// is the total amount of data queued in the network buffer for reading. If no data is queued in the network buffer, <see cref="Available"/> returns 0.
        /// </para>
        /// <para>
        /// If the remote host shuts down or closes the connection, <see cref="Available"/> can throw a <see cref="SocketException"/>. If you receive a <see cref="SocketException"/>, use the 
        /// <see cref="SocketException.ErrorCode"/> property to obtain the specific error code. After you have obtained this code, refer to the Windows Sockets version 2 API 
        /// error code documentation in the MSDN library for a detailed description of the error.
        /// </para>
        /// </remarks>
        public int Available
        {
            get
            {
                if (m_Handle == -1)
                {
                    throw new ObjectDisposedException();
                }

                uint cBytes = 0;

                NativeSocket.ioctl(this, NativeSocket.FIONREAD, ref cBytes);

                return (int)cBytes;
            }
        }

        private EndPoint GetEndPoint(bool fLocal)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            if (m_localEndPoint == null)
            {
                m_localEndPoint = new IPEndPoint(IPAddress.Any, 0);
            }

            EndPoint endPoint = null;

            if (fLocal)
            {
                NativeSocket.getsockname(this, out endPoint);
            }
            else
            {
                NativeSocket.getpeername(this, out endPoint);
            }

            if (fLocal)
            {
                m_localEndPoint = endPoint;
            }

            return endPoint;
        }

        /// <summary>
        /// Gets the local endpoint.
        /// </summary>
        /// <value>The EndPoint that the Socket is using for communications.</value>
        /// <remarks>
        /// <para>
        /// The LocalEndPoint property gets an <see cref="EndPoint"/> that contains the local IP address and port number to which your <see cref="Socket"/> is bound. You must cast this 
        /// <see cref="EndPoint"/> to an <see cref="IPEndPoint"/> before retrieving any information. You can then call the <see cref="IPEndPoint.Address"/> method to retrieve the local <see cref="IPAddress"/>, and the 
        /// <see cref="IPEndPoint.Port"/> method to retrieve the local port number.
        /// </para>
        /// <para>
        /// The LocalEndPoint property is usually set after you make a call to the <see cref="Bind"/> method. If you allow the system to assign your socket's local IP address and
        /// port number, the LocalEndPoint property will be set after the first I/O operation. For connection-oriented protocols, the first I/O operation would be a call 
        /// to the Connect or <see cref="Accept"/> method. For connectionless protocols, the first I/O operation would be any of the send or receive calls.
        /// </para>
        /// </remarks>
        public EndPoint LocalEndPoint
        {
            get
            {
                return GetEndPoint(true);
            }
        }

        /// <summary>
        /// Gets the remote endpoint.
        /// </summary>
        /// <value>
        /// The <see cref="EndPoint"/> with which the <see cref="Socket"/> is communicating.
        /// </value>
        /// <remarks>
        /// <para>
        /// If you are using a connection-oriented protocol, the RemoteEndPoint property gets the <see cref="EndPoint"/> that contains the remote IP address and port number to
        /// which the <see cref="Socket"/> is connected. If you are using a connectionless protocol, RemoteEndPoint contains the default remote IP address and port number with 
        /// which the <see cref="Socket"/> will communicate. You must cast this <see cref="EndPoint"/> to an <see cref="IPEndPoint"/> before retrieving any information. You can then call the 
        /// <see cref="IPEndPoint.Address"/> method to retrieve the remote <see cref="IPAddress"/>, and the <see cref="IPEndPoint.Port"/> method to retrieve the remote port number.
        /// </para>
        /// <para>
        /// The RemoteEndPoint is set after a call to either <see cref="Accept"/> or <see cref="Connect"/>. If you try to access this property earlier, RemoteEndPoint will throw a <see cref="SocketException"/>. 
        /// If you receive a <see cref="SocketException"/>, use the <see cref="SocketException.ErrorCode"/> property to obtain the specific error code. After you have obtained this code, refer
        /// to the Windows Sockets version 2 API error code documentation in the MSDN library for a detailed description of the error.
        /// </para>
        /// </remarks>
        public EndPoint RemoteEndPoint
        {
            get
            {
                return GetEndPoint(false);
            }
        }

        /// <summary>
        /// Gets or sets a value that specifies the amount of time after which a synchronous <see cref="Receive(Byte[])"/> call will time out.
        /// </summary>
        /// <value>
        /// The time-out value, in milliseconds. The default value is 0, which indicates an infinite time-out period. Specifying -1 also indicates an infinite time-out period.
        /// </value>
        /// <remarks>
        /// This option applies to synchronous <see cref="Receive(Byte[])"/> calls only. If the time-out period is exceeded, the <see cref="Receive(Byte[])"/> method will throw a <see cref="SocketException"/>.
        /// </remarks>
        public int ReceiveTimeout
        {
            get
            {
                return m_recvTimeout;
            }

            set
            {
                if (value < Timeout.Infinite) throw new ArgumentOutOfRangeException();

                // desktop implementation treats 0 as infinite
                m_recvTimeout = ((value == 0) ? Timeout.Infinite : value);
            }
        }

        /// <summary>
        /// Gets or sets a value that specifies the amount of time after which a synchronous <see cref="Send(Byte[])"/> call will time out.
        /// </summary>
        /// <value>
        /// The time-out value, in milliseconds. If you set the property with a value between 1 and 499, 
        /// the value will be changed to 500. The default value is 0, which indicates an infinite time-out period. Specifying -1 also indicates an infinite time-out period.
        /// </value>
        /// <remarks>
        /// This option applies to synchronous <see cref="Send(Byte[])"/> calls only. If the time-out period is exceeded, the <see cref="Send(Byte[])"/> method will throw a <see cref="SocketException"/>.
        /// </remarks>
        public int SendTimeout
        {
            get
            {
                return m_sendTimeout;
            }

            set
            {
                if (value < Timeout.Infinite) throw new ArgumentOutOfRangeException();

                // desktop implementation treats 0 as infinite
                m_sendTimeout = ((value == 0) ? Timeout.Infinite : value);
            }
        }

        /// <summary>
        /// Gets the type of the <see cref="Socket"/>.
        /// </summary>
        /// <value>
        /// One of the <see cref="SocketType"/> values.
        /// </value>
        /// <remarks>
        /// <see cref="SocketType"/> is read-only and is set when the <see cref="Socket"/> is created.
        /// </remarks>
        public SocketType SocketType
        {
            get
            {
                return _socketType;
            }
        }

        /// <summary>
        /// Associates a <see cref="Socket"/> with a local endpoint.
        /// </summary>
        /// <param name="localEP">The local <see cref="EndPoint"/> to associate with the <see cref="Socket"/>.</param>
        /// <remarks>
        /// <para>
        /// Use the Bind method if you need to use a specific local endpoint. You must call Bind before you can call the <see cref="Listen"/> method. You do not need to call Bind
        /// before using the <see cref="Connect"/> method unless you need to use a specific local endpoint. You can use the Bind method on both connectionless and 
        /// connection-oriented protocols.
        /// </para>
        /// <para>
        /// Before calling Bind, you must first create the local <see cref="IPEndPoint"/> from which you intend to communicate data. If you do not care which local address is
        /// assigned, you can create an <see cref="IPEndPoint"/> using <see cref="IPAddress.Any"/> as the address parameter, and the underlying service provider will assign the most 
        /// appropriate network address. This might help simplify your application if you have multiple network interfaces. If you do not care which local port 
        /// is used, you can create an <see cref="IPEndPoint"/> using 0 for the port number. In this case, the service provider will assign an available port number between 1024 and 5000.
        /// </para>
        /// <para>
        /// If you use the above approach, you can discover what local network address and port number has been assigned by calling the <see cref="LocalEndPoint"/>. If you are using a 
        /// connection-oriented protocol, <see cref="LocalEndPoint"/> will not return the locally assigned network address until after you have made a call to the <see cref="Connect"/> method. If 
        /// you are using a connectionless protocol, you will not have access to this information until you have completed a send or receive.
        /// </para>
        /// <c>If you intend to receive multicast datagrams, you must call the Bind method with a multicast port number.</c>
        /// <c>You must call the Bind method if you intend to receive connectionless datagrams using the ReceiveFrom method.</c> 
        /// </remarks>
        public void Bind(EndPoint localEP)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            EndPoint endPointSnapshot = localEP;
            IPEndPoint ipSnapshot = localEP as IPEndPoint;

            // take a snapshot that will make it immutable and not derived.
            ipSnapshot = ipSnapshot.Snapshot();
            // TODO IPv6
            //endPointSnapshot = RemapIPEndPoint(ipSnapshot);

            NativeSocket.bind(this, endPointSnapshot);

            if (_rightEndPoint == null)
            {
                // save a copy of the EndPoint
                _rightEndPoint = endPointSnapshot;
            }

            m_localEndPoint = localEP;
        }

        /// <summary>
        /// Establishes a connection to a remote host.
        /// </summary>
        /// <param name="remoteEP">An <see cref="EndPoint"/> that represents the remote device.</param>
        /// <remarks>
        /// <para>
        /// If you are using a connection-oriented protocol such as TCP, the Connect method synchronously establishes a network connection between <see cref="LocalEndPoint"/>
        /// and the specified remote endpoint. If you are using a connectionless protocol, Connect establishes a default remote host. After you call Connect, you can 
        /// send data to the remote device with the <see cref="Send(Byte[])"/> method, or receive data from the remote device with the <see cref="Receive(Byte[])"/> method.
        /// </para>
        /// <para>
        /// If you are using a connectionless protocol such as UDP, you do not have to call Connect before sending and receiving data. You can use <see cref="SendTo(byte[], EndPoint)"/> and 
        /// <see cref="ReceiveFrom(byte[], int, int, SocketFlags, ref EndPoint)"/> to synchronously communicate with a remote host. If you do call Connect, any datagrams that arrive from an address other than the 
        /// specified default will be discarded. If you want to set your default remote host to a broadcast address, you must first call the <see cref="SetSocketOption(SocketOptionLevel, SocketOptionName, bool)"/> method and 
        /// set the socket option to <see cref="SocketOptionName.Broadcast"/>, or Connect will throw a SocketException. If you receive a <see cref="SocketException"/>, use the 
        /// <see cref="SocketException.ErrorCode"/> property to obtain the specific error code. After you have obtained this code, refer to the Windows Sockets version 2 API 
        /// error code documentation in the MSDN library for a detailed description of the error.
        /// </para>
        /// <para>
        /// <c>
        /// If you are using a connection-oriented protocol and did not call Bind before calling Connect, the underlying service provider will assign the local network address and port number. If you are using a connectionless protocol, the service provider will not assign a local network address and port number until you complete a send or receive operation. 
        /// If you want to change the default remote host, call Connect again with the desired endpoint.
        /// </c>
        /// </para>
        /// </remarks>
        public void Connect(EndPoint remoteEP)
        {
            try
            {
                if (m_Handle == -1) {
                    throw new ObjectDisposedException();
                }

                EndPoint endPointSnapshot = remoteEP;
                Snapshot(ref endPointSnapshot);

                if (m_fBlocking) {
                    // blocking connect
                    _nonBlockingConnectInProgress = false;
                } else {
                    // non blocking connect
                    _nonBlockingConnectInProgress = true;
                    _nonBlockingConnectRightEndPoint = endPointSnapshot;
                }

                // Unhandled exceptions were causing nanoFramework applications to crash
                try
                {
                    NativeSocket.connect(this, endPointSnapshot, !m_fBlocking);
                }
                catch (Exception ec)
                {
                    Debug.WriteLine($"System.Net - Connect() - NativeSocket.connect() - Exception caught {ec.GetType().Name} ");
                    throw new SocketException(SocketError.SocketError);
                }

                try
                {
                    if (m_fBlocking)
                    {
                        // if we are on blocking connect
                        //Poll(-1, SelectMode.SelectWrite);                                 // Original code would polled until connection was established or exception thrown - this hung nanoFramework applications
                        if (!Poll(1500000, SelectMode.SelectWrite))                         // Workaround - Use this timeout for now.  Fix for this needs to be implemented in the nanoFramework interpreter 
                        {
                            Debug.WriteLine($"System.Net - Connect() - call to Poll() timed out");
                            throw new SocketException(SocketError.TimedOut);
                        }
                    }
                }
                catch (Exception ep)
                {
                    Debug.WriteLine($"System.Net - Connect() - Exception caught {ep.GetType().Name} ");
                    throw new SocketException(SocketError.SocketError);
                }


                if (_rightEndPoint == null) {
                    // save a copy of the EndPoint
                    _rightEndPoint = endPointSnapshot;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Closes the <see cref="Socket"/> connection and releases all associated resources.
        /// </summary>
        /// <remarks>
        /// The Close method closes the remote host connection and releases all managed and unmanaged resources associated with the Socket. 
        /// Upon closing, the Connected property is set to false.
        /// </remarks>
        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        /// <summary>
        /// Places a <see cref="Socket"/> in a listening state.
        /// </summary>
        /// <param name="backlog">The maximum length of the pending connections queue.</param>
        /// <remarks>
        /// <para>
        /// Listen causes a connection-oriented <see cref="Socket"/> to listen for incoming connection attempts. The backlog parameter specifies the number of incoming 
        /// connections that can be queued for acceptance. To determine the maximum number of connections you can specify, retrieve the <see cref="SocketOptionName.MaxConnections"/> value. 
        /// Listen does not block.
        /// </para>
        /// <para>
        /// If you receive a <see cref="SocketException"/>, use the <see cref="SocketException.ErrorCode"/> property to obtain the specific error code. After you have obtained this code, refer to the 
        /// Windows Sockets version 2 API error code documentation in the MSDN library for a detailed description of the error. Use <see cref="Accept"/> or BeginAccept 
        /// to accept a connection from the queue.
        /// </para>
        /// <para>
        /// <c>You must call the <see cref="Bind"/> method before calling Listen, or Listen will throw a <see cref="SocketException"/>.</c>
        /// </para>
        /// </remarks>
        public void Listen(int backlog)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            NativeSocket.listen(this, backlog);
        }

        /// <summary>
        /// Creates a new <see cref="Socket"/> for a newly created connection.
        /// </summary>
        /// <remarks>
        /// <para>Accept synchronously extracts the first pending connection request from the connection request queue of the listening socket, and then creates and 
        /// returns a new <see cref="Socket"/>. You cannot use this returned <see cref="Socket"/> to accept any additional connections from the connection queue. However, you can call the 
        /// <see cref="RemoteEndPoint"/> method of the returned <see cref="Socket"/> to identify the remote host's network address and port number.
        /// </para>
        /// <para>
        /// <c>Before calling the Accept method, you must first call the Listen method to listen for and queue incoming connection requests.</c>
        /// </para>
        /// </remarks>
        /// <returns>A <see cref="Socket"/> for a newly created connection.</returns>
        public Socket Accept()
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            int socketHandle;

            if (m_fBlocking)
            {
                Poll(-1, SelectMode.SelectRead);
            }

            socketHandle = NativeSocket.accept(this);

            Socket socket = new Socket(socketHandle);

            // creating a socket from Accept() is only possible for Stream sockets
            // have to set the type here
            socket._socketType = SocketType.Stream;

            socket.m_localEndPoint = this.m_localEndPoint;

            return socket;
        }

        /// <summary>
        /// Sends data to a connected <see cref="Socket"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent</param>
        /// <returns>The number of bytes sent to the <see cref="Socket"/>.</returns>
        /// <remarks>
        /// <para><see cref="Send(Byte[])"/> synchronously sends data to the remote host specified in the <see cref="Connect"/> or <see cref="Accept"/> method and returns the number of bytes successfully sent. <see cref="Send(Byte[])"/> 
        /// can be used for both connection-oriented and connectionless protocols.</para>
        /// <para>This overload requires a buffer that contains the data you want to send. The <see cref="SocketFlags"/> value defaults to 0, the buffer offset defaults to 0, and the number of bytes to send defaults to the size of the buffer.</para>
        /// <para>If you are using a connectionless protocol, you must call <see cref="Connect"/> before calling this method, or <see cref="o:Send"/> will throw a <see cref="SocketException"/>. If you are using a connection-oriented protocol, you must either 
        /// use <see cref="Connect"/> to establish a remote host connection, or use <see cref="Accept"/> to accept an incoming connection.</para>
        /// <para>If you are using a connectionless protocol and plan to send data to several different hosts, you should use the <see cref="SendTo(byte[], EndPoint)"/> method. If you do not use the 
        /// SendTo method, you will have to call Connect before each call to Send. You can use SendTo even after you have established a default remote host with 
        /// Connect. You can also change the default remote host prior to calling Send by making another call to Connect.</para>
        /// <para>
        /// If you are using a connection-oriented protocol, <see cref="o:SendTo()"/> will block until all of the bytes in the buffer are sent, unless a time-out was set by using 
        /// <see cref="SendTimeout"/>. If the time-out value was exceeded, the <see cref="o:SendTo()"/> call will throw a <see cref="SocketException"/>. In nonblocking mode, Send may complete 
        /// successfully even if it sends less than the number of bytes in the buffer. It is your application's responsibility to keep track of the number of bytes sent and 
        /// to retry the operation until the application sends the bytes in the buffer. There is also no guarantee that the data you send will appear on the network 
        /// immediately. To increase network efficiency, the underlying system may delay transmission until a significant amount of outgoing data is collected. A 
        /// successful completion of the <see cref="Send(byte[])"/> method means that the underlying system has had room to buffer your data for a network send.
        /// </para>
        /// <note type="important">
        /// If you receive a SocketException, use the <see cref="SocketException.ErrorCode"/> property to obtain the specific error code. After you have obtained this code, 
        /// refer to the Windows Sockets version 2 API error code documentation in the MSDN library for a detailed description of the error.
        /// </note>
        /// <note type="important">
        /// The successful completion of a send does not indicate that the data was successfully delivered. 
        /// If no buffer space is available within the transport system to hold the data to be transmitted, 
        /// send will block unless the socket has been placed in nonblocking mode.
        /// </note>
        /// </remarks>
        /// <seealso cref="o:Send"/>
        /// <seealso cref="Socket"/>
        /// <seealso cref="Sockets"/>
        public int Send(byte[] buffer)
        {
            return Send(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None);
        }

        /// <summary>
        /// Sends data to a connected <see cref="Socket"/> using the specified <see cref="SocketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent</param>
        /// <param name="socketFlags"></param>
        /// <returns>The number of bytes sent to the <see cref="Socket"/>.</returns>
        /// <remarks>
        /// <para><see cref="o:Send()"/> synchronously sends data to the remote host specified in the <see cref="Connect"/> or <see cref="Accept"/> method and returns the number of bytes successfully sent. <see cref="o:Send()"/> 
        /// can be used for both connection-oriented and connectionless protocols.</para>
        /// <para>his overload requires a buffer that contains the data you want to send and a bitwise combination of SocketFlags. 
        /// The buffer offset defaults to 0, and the number of bytes to send defaults to the size of the buffer. If you specify the 
        /// DontRoute flag as the socketflags parameter value, the data you are sending will not be routed.
        /// </para>
        /// <note type="important">
        /// You must ensure that the size of your buffer does not exceed the maximum packet size of the underlying service provider. 
        /// If it does, the datagram will not be sent and <see cref="o:Send"/> will throw a <see cref="SocketException"/>. If you receive a 
        /// <see cref="SocketException"/>, use the <see cref="SocketException.ErrorCode"/> property to obtain the specific error code. After you have obtained this code, refer to the Windows Sockets version 2 API error code documentation in the MSDN library for a detailed description of the error.
        /// </note>
        /// <note type="important">
        /// The successful completion of a send does not indicate that the data was successfully delivered. 
        /// If no buffer space is available within the transport system to hold the data to be transmitted, 
        /// send will block unless the socket has been placed in nonblocking mode.
        /// </note>
        /// </remarks>
        /// <seealso cref="o:Send"/>
        /// <seealso cref="Socket"/>
        /// <seealso cref="Sockets"/>
        public int Send(byte[] buffer, SocketFlags socketFlags)
        {
            return Send(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags);
        }

        /// <summary>
        /// Sends the specified number of bytes of data to a connected <see cref="Socket"/>, using the specified <see cref="SocketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent</param>
        /// <param name="size"></param>
        /// <param name="socketFlags"></param>
        /// <returns>The number of bytes sent to the <see cref="Socket"/>.</returns>
        /// <remarks>
        /// <para><see cref="o:Send()"/> synchronously sends data to the remote host specified in the <see cref="Connect"/> or <see cref="Accept"/> method and returns the number of bytes successfully sent. <see cref="o:Send()"/> 
        /// can be used for both connection-oriented and connectionless protocols.</para>
        /// <para>This overload requires a buffer that contains the data you want to send, the number of bytes you want to send, 
        /// and a bitwise combination of any <see cref="SocketFlags"/>. If you specify the <see cref="SocketFlags.DontRoute"/> flag as the socketflags parameter, the data you are sending will not be routed.</para>
        /// <note type="important">
        /// You must ensure that the size of your buffer does not exceed the maximum packet size of the underlying service provider. 
        /// If it does, the datagram will not be sent and <see cref="o:Send"/> will throw a <see cref="SocketException"/>. If you receive a 
        /// <see cref="SocketException"/>, use the <see cref="SocketException.ErrorCode"/> property to obtain the specific error code. After you have obtained this code, refer to the Windows Sockets version 2 API error code documentation in the MSDN library for a detailed description of the error.
        /// </note>
        /// <note type="important">
        /// The successful completion of a send does not indicate that the data was successfully delivered. 
        /// If no buffer space is available within the transport system to hold the data to be transmitted, 
        /// send will block unless the socket has been placed in nonblocking mode.
        /// </note>
        /// </remarks>
        /// <seealso cref="o:Send"/>
        /// <seealso cref="Socket"/>
        /// <seealso cref="Sockets"/>
        public int Send(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return Send(buffer, 0, size, socketFlags);
        }

        /// <summary>
        /// Sends the specified number of bytes of data to a connected <see cref="Socket"/>, starting at the specified offset, and using the specified <see cref="SocketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent</param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="socketFlags"></param>
        /// <returns>The number of bytes sent to the <see cref="Socket"/>.</returns>
        /// <remarks>
        /// <para><see cref="o:Send()"/> synchronously sends data to the remote host specified in the <see cref="Connect"/> or <see cref="Accept"/> method and returns the number of bytes successfully sent. <see cref="o:Send()"/> 
        /// can be used for both connection-oriented and connectionless protocols.</para>
        /// <para>In this overload, if you specify the <see cref="SocketFlags.DontRoute"/> flag as the socketflags parameter, the data you are sending will not be routed.</para>
        /// <note type="important">
        /// The successful completion of a send does not indicate that the data was successfully delivered. 
        /// If no buffer space is available within the transport system to hold the data to be transmitted, 
        /// send will block unless the socket has been placed in nonblocking mode.
        /// </note>
        /// </remarks>
        /// <seealso cref="o:Send"/>
        /// <seealso cref="Socket"/>
        /// <seealso cref="Sockets"/>
        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            return NativeSocket.send(this, buffer, offset, size, (int)socketFlags, m_sendTimeout);
        }

        /// <summary>
        /// Sends data to the specified endpoint.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
        /// <param name="offset">The <see cref="EndPoint"/> that represents the destination for the data.</param>
        /// <param name="size">The number of bytes to send.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <param name="remoteEP">The <see cref="EndPoint"/> that represents the destination location for the data.</param>
        /// <returns>The number of bytes sent.</returns>
        public int SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            EndPoint endPointSnapshot = remoteEP;

            int bytesTransferred = NativeSocket.sendto(this, buffer, offset, size, (int)socketFlags, m_sendTimeout, endPointSnapshot);

            if (_rightEndPoint == null)
            {
                // save a copy of the EndPoint
                _rightEndPoint = endPointSnapshot;
            }

            return bytesTransferred;
        }

        /// <summary>
        /// Sends the specified number of bytes of data to the specified endpoint using the specified SocketFlags.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
        /// <param name="size">The number of bytes to send.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <param name="remoteEP">The <see cref="EndPoint"/> that represents the destination location for the data.</param>
        /// <returns>The number of bytes sent.</returns>
        public int SendTo(byte[] buffer, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return SendTo(buffer, 0, size, socketFlags, remoteEP);
        }

        /// <summary>
        /// Sends data to a specific endpoint using the specified <see cref="SocketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <param name="remoteEP">The <see cref="EndPoint"/> that represents the destination location for the data.</param>
        /// <returns>The number of bytes sent.</returns>
        /// <remarks>
        /// <para>
        /// In this overload, the buffer offset defaults to 0, and the number of bytes to send defaults to the size of the buffer. If you specify the <see cref="SocketFlags.DontRoute"/> flag as the socketflags parameter, the data you are sending will not be routed.
        /// </para>
        /// </remarks>
        public int SendTo(byte[] buffer, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return SendTo(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags, remoteEP);
        }

        /// <summary>
        /// Sends data to the specified endpoint.
        /// </summary>
        /// <param name="buffer">n array of type Byte that contains the data to be sent.</param>
        /// <param name="remoteEP">The <see cref="EndPoint"/> that represents the destination location for the data.</param>
        /// <returns>The number of bytes sent.</returns>
        /// <remarks>
        /// In this overload, the buffer offset defaults to 0, the number of bytes to send defaults to the size of the buffer parameter, and the SocketFlags value defaults to 0.
        /// </remarks>
        public int SendTo(byte[] buffer, EndPoint remoteEP)
        {
            return SendTo(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None, remoteEP);
        }

        /// <summary>
        /// Receives data from a bound <see cref="Socket"/> into a receive buffer.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for the received data.</param>
        /// <returns>The number of bytes received.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="Receive(byte[])"/> method reads data into the buffer parameter and returns the number of bytes successfully read. You can call <see cref="Receive(byte[])"/> from both connection-oriented 
        /// and connectionless sockets.
        /// </para>
        /// <para>
        /// This overload only requires you to provide a receive buffer. The buffer offset defaults to 0, the size defaults to the length of the buffer parameter, and the 
        /// <see cref="SocketFlags"/> value defaults to <see cref="SocketFlags.None"/>.
        /// </para>
        /// <para>
        /// If no data is available for reading, the <see cref="Receive(byte[])"/> method will block until data is available, unless a time-out value was set by using <see cref="ReceiveTimeout"/>. If 
        /// the time-out value was exceeded, the <see cref="Receive(Byte[])"/> call will throw a <see cref="SocketException"/>. If you are in non-blocking mode, and there is no data available in the
        /// in the protocol stack buffer, the <see cref="Receive(Byte[])"/> method will complete immediately and throw a <see cref="SocketException"/>. You can use the <see cref="Available"/> property to 
        /// determine if data is available for reading. When <see cref="Available"/> is non-zero, retry the receive operation.
        /// </para>
        /// <para>If you are using a connectionless <see cref="Socket"/>, <see cref="Receive(Byte[])"/> will read the first queued datagram from the destination address you specify in the Connect method. If 
        /// the datagram you receive is larger than the size of the buffer parameter, buffer gets filled with the first part of the message, the excess data is lost and a 
        /// SocketException is thrown.
        /// </para>
        /// </remarks>
        public int Receive(byte[] buffer)
        {
            return Receive(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None);
        }

        /// <summary>
        /// Receives data from a bound <see cref="Socket"/> into a receive buffer, using the specified <see cref="SocketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for the received data.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <returns>The number of bytes received.</returns>
        /// <remarks>
        /// <para>The <see cref="Receive(byte[])"/> method reads data into the buffer parameter and returns the number of bytes successfully read. You can call <see cref="Receive(byte[])"/> from both connection-oriented 
        /// and connectionless sockets.</para>
        /// <para>
        /// This overload only requires you to provide a receive buffer and the necessary <see cref="SocketFlags"/>. The buffer offset defaults to 0, and the size defaults to the length of the byte parameter.
        /// </para>
        /// </remarks>
        public int Receive(byte[] buffer, SocketFlags socketFlags)
        {
            return Receive(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags);
        }

        /// <summary>
        /// Receives the specified number of bytes of data from a bound <see cref="Socket"/> into a receive buffer, using the specified <see cref="SocketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <returns>The number of bytes received.</returns>
        /// <remarks>
        /// <para>The <see cref="Receive(byte[])"/> method reads data into the buffer parameter and returns the number of bytes successfully read. You can call Receive from 
        /// both connection-oriented and connectionless sockets.</para>
        /// <para>This overload only requires you to provide a receive buffer, the number of bytes you want to receive, and the necessary <see cref="SocketFlags"/>.</para>
        /// <para>If no data is available for reading, the Receive method will block until data is available, unless a time-out value was set by using 
        /// <see cref="ReceiveTimeout"/>. If the time-out value was exceeded, the <see cref="Receive(byte[])"/> call will throw a <see cref="SocketException"/>. If you are in non-blocking 
        /// mode, and there is no data available in the in the protocol stack buffer, The <see cref="Receive(byte[])"/> method will complete immediately and throw a <see cref="SocketException"/>. 
        /// You can use the <see cref="Available"/> property to determine if data is available for reading. When <see cref="Available"/> is non-zero, retry your receive operation.</para>
        /// </remarks>
        public int Receive(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return Receive(buffer, 0, size, socketFlags);
        }


        /// <summary>
        /// Receives the specified number of bytes from a bound <see cref="Socket"/> into the specified offset position of the receive buffer, using the specified <see cref="SocketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for the received data.</param>
        /// <param name="offset">The location in buffer to store the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <returns>The number of bytes received.</returns>
        /// <remarks>
        /// <para>The <see cref="Receive(byte[])"/> method reads data into the buffer parameter and returns the number of bytes successfully read. You can call <see cref="Receive(byte[])"/> from both connection-
        /// oriented and connectionless sockets.</para>
        /// </remarks>
        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            // Unhandled exceptions caused nanoFramework applications to crash here
            try
            {
                return NativeSocket.recv(this, buffer, offset, size, (int)socketFlags, m_recvTimeout);
            }
            catch
            {
                throw new SocketException(SocketError.SocketError);
            }
        }

        /// <summary>
        /// Receives the specified number of bytes of data into the specified location of the data buffer, using the specified <see cref="SocketFlags"/>, and stores the endpoint.
        /// </summary>
        /// <param name="buffer">An array of type <see cref="Byte"/> that is the storage location for received data.</param>
        /// <param name="offset">The position in the buffer parameter to store the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <param name="remoteEP">An <see cref="EndPoint"/>, passed by reference, that represents the remote server.</param>
        /// <returns>The number of bytes received.</returns>
        /// <remarks>
        /// The <see cref="ReceiveFrom(byte[], int, int, SocketFlags, ref EndPoint)"/> method reads data into the buffer parameter, returns the number of bytes successfully read, and captures the remote host endpoint from which the data was sent. This method is useful if you intend to receive connectionless datagrams from an unknown host or multiple hosts.
        /// 
        /// With connectionless protocols, <see cref="ReceiveFrom(byte[], int, int, SocketFlags, ref EndPoint)"/> will read the first enqueued datagram received into the local network buffer.If the datagram you receive is larger than the size of buffer, the <see cref="ReceiveFrom(byte[], int, int, SocketFlags, ref EndPoint)"/> method will fill buffer with as much of the message as is possible, and throw a <see cref="SocketException"/>.If you are using an unreliable protocol, the excess data will be lost.If you are using a reliable protocol, the excess data will be retained by the service provider and you can retrieve it by calling the <see cref="ReceiveFrom(byte[], int, int, SocketFlags, ref EndPoint)"/> method with a large enough buffer.
        /// 
        /// If no data is available for reading, the <see cref="ReceiveFrom(byte[], int, int, SocketFlags, ref EndPoint)"/> method will block until data is available.If you are in non-blocking mode, and there is no data available in the in the protocol stack buffer, the <see cref="ReceiveFrom(byte[], int, int, SocketFlags, ref EndPoint)"/> method will complete immediately and throw a <see cref="SocketException"/>. You can use the <see cref="Available"/> property to determine if data is available for reading. When <see cref="Available"/> is non-zero, retry the receive operation.
        /// 
        /// Although <see cref="ReceiveFrom(byte[], int, int, SocketFlags, ref EndPoint)"/> is intended for connectionless protocols, you can use a connection-oriented protocol as well.If you choose to do so, you must first either establish a remote host connection by calling the Connect method or accept an incoming remote host connection by calling the <see cref="Accept"/> method. If you do not establish or accept a connection before calling the <see cref="ReceiveFrom(byte[], int, int, SocketFlags, ref EndPoint)"/> method, you will get a <see cref="SocketException"/>.You can also establish a default remote host for a connectionless protocol prior to calling the <see cref="ReceiveFrom(byte[], int, int, SocketFlags, ref EndPoint)"/> method.In either of these cases, the <see cref="ReceiveFrom(byte[], int, int, SocketFlags, ref EndPoint)"/> method will ignore the remoteEP parameter and only receive data from the connected or default remote host.
        /// 
        /// With connection-oriented sockets, <see cref="ReceiveFrom(byte[], int, int, SocketFlags, ref EndPoint)"/> will read as much data as is available up to the amount of bytes specified by the size parameter. If the remote host shuts down the Socket connection with the Shutdown method, and all available data has been Received, the <see cref="ReceiveFrom(byte[], int, int, SocketFlags, ref EndPoint)"/> method will complete immediately and return zero bytes.
        /// </remarks>
        public int ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            if (_rightEndPoint == null)
            {
                // socket must have connection established or previously accepted a connection 
                throw new SocketException(SocketError.NotConnected);
            }

            EndPoint endPointSnapshot = remoteEP;
            Snapshot(ref endPointSnapshot);

            int bytesTransferred = 0;

            bytesTransferred = NativeSocket.recvfrom(this, buffer, offset, size, (int)socketFlags, m_recvTimeout, ref remoteEP);

            if (!remoteEP.Equals(endPointSnapshot))
            {
                // no need to create a new EndPoint here if it's different from the orignal
                // because the interpreter has already created a new instance of an IPEndPoint

                if (_rightEndPoint == null)
                {
                    // save a copy of the EndPoint
                    _rightEndPoint = remoteEP;
                }
            }

            return bytesTransferred;
        }

        /// <summary>
        /// Receives the specified number of bytes into the data buffer, using the specified <see cref="SocketFlags"/>, and stores the endpoint.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <param name="remoteEP">An <see cref="EndPoint"/>, passed by reference, that represents the remote server.</param>
        /// <returns>The number of bytes received.</returns>
        public int ReceiveFrom(byte[] buffer, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            return ReceiveFrom(buffer, 0, size, socketFlags, ref remoteEP);
        }

        /// <summary>
        /// Receives a datagram into the data buffer, using the specified <see cref="SocketFlags"/>, and stores the endpoint.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <param name="remoteEP">An <see cref="EndPoint"/>, passed by reference, that represents the remote server.</param>
        /// <returns>The number of bytes received.</returns>
        public int ReceiveFrom(byte[] buffer, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            return ReceiveFrom(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags, ref remoteEP);
        }

        /// <summary>
        /// Receives a datagram into the data buffer and stores the endpoint.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="remoteEP">An <see cref="EndPoint"/>, passed by reference, that represents the remote server.</param>
        /// <returns>The number of bytes received.</returns>
        /// <remarks>
        /// <para>The <see cref="o:ReceiveFrom"/> method reads data into the buffer parameter, returns the number of bytes successfully read, 
        /// and captures the remote host endpoint from which the data was sent. This method is useful if you intend to receive 
        /// connectionless datagrams from an unknown host or multiple hosts.</para>
        /// <para>This overload only requires you to provide a receive buffer, and an EndPoint that represents the remote host.
        /// The buffer offset defaults to 0. The size defaults to the length of the buffer parameter and the socketFlags value 
        /// defaults to None.
        /// </para>
        /// <note type="important">
        /// Before calling <see cref="o:ReceiveFrom"/>, you must explicitly <see cref="Bind(EndPoint)"/> the <see cref="Socket"/> to a local endpoint using the <see cref="Bind"/> method. If you do not, <see cref="o:ReceiveFrom"/> will 
        /// throw a <see cref="SocketException"/>.
        /// </note>
        /// <note type="important">
        /// The <see cref="AddressFamily"/> of the <see cref="EndPoint"/> used in <see cref="o:ReceiveFrom"/> needs to match the <see cref="AddressFamily"/> of the <see cref="EndPoint"/> used in <see cref="o:SendTo"/>.
        /// </note>
        /// </remarks>
        public int ReceiveFrom(byte[] buffer, ref EndPoint remoteEP)
        {
            return ReceiveFrom(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None, ref remoteEP);
        }

        /// <summary>
        /// Sets the specified <see cref="Socket"/> option to the specified integer value.
        /// </summary>
        /// <param name="optionLevel">One of the <see cref="SocketOptionLevel"/> values.</param>
        /// <param name="optionName">One of the <see cref="SocketOptionName"/> values.</param>
        /// <param name="optionValue">A value of the option.</param>
        /// <remarks>
        /// <para><see cref="Socket"/> options determine the behavior of the current <see cref="Socket"/>. For an option with a Boolean data type, specify a nonzero value to enable the option, and a 
        /// zero value to disable the option. For an option with an integer data type, specify the appropriate value. <see cref="Socket"/> options are grouped by level of protocol support.
        /// </para>
        /// </remarks>
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            byte[] val = new byte[4] { (byte)(optionValue >> 0), (byte)(optionValue >> 8), (byte)(optionValue >> 16), (byte)(optionValue >> 24) };

            switch (optionName)
            {
                case SocketOptionName.SendTimeout:
                    m_sendTimeout = optionValue;
                    break;
                case SocketOptionName.ReceiveTimeout:
                    m_recvTimeout = optionValue;
                    break;
            }

            NativeSocket.setsockopt(this, (int)optionLevel, (int)optionName, val);
        }

        /// <summary>
        /// Sets the specified <see cref="Socket"/> option to the specified Boolean value.
        /// </summary>
        /// <param name="optionLevel">One of the <see cref="SocketOptionLevel"/> values.</param>
        /// <param name="optionName">One of the <see cref="SocketOptionName"/> values.</param>
        /// <param name="optionValue">The value of the option, represented as a Boolean.</param>
        /// <remarks>
        /// </remarks>
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
        {
            SetSocketOption(optionLevel, optionName, (optionValue ? 1 : 0));
        }

        /// <summary>
        /// Sets the specified <see cref="Socket"/> option to the specified value, represented as a byte array.
        /// </summary>
        /// <param name="optionLevel">One of the <see cref="SocketOptionLevel"/> values.</param>
        /// <param name="optionName">One of the <see cref="SocketOptionName"/> values.</param>
        /// <param name="optionValue">An array of type Byte that represents the value of the option.</param>
        /// <remarks>
        /// <para><see cref="Socket"/> options determine the behavior of the current <see cref="Socket"/>. Use this overload to set those <see cref="Socket"/> options that require a byte array as an option value.
        /// </para>
        /// </remarks>
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            NativeSocket.setsockopt(this, (int)optionLevel, (int)optionName, optionValue);
        }

        /// <summary>
        /// Returns the value of a <see cref="Socket"/> option.
        /// </summary>
        /// <param name="optionLevel">One of the <see cref="SocketOptionLevel"/> values.</param>
        /// <param name="optionName">One of the <see cref="SocketOptionName"/> values.</param>
        /// <returns>
        /// An object that represents the value of the option. When the optionName parameter is set to <see cref="SocketOptionName.Linger"/> the return value is an instance of the LingerOption
        /// class. When optionName is set to <see cref="SocketOptionName.AddMembership"/> or <see cref="SocketOptionName.DropMembership"/>, the return value is an instance of the MulticastOption class. When optionName is 
        /// any other value, the return value is an integer.
        /// </returns>
        /// <remarks>
        /// <see cref="Socket"/> options determine the behavior of the current <see cref="Socket"/>. Use this overload to get the <see cref="SocketOptionName.Linger"/>, <see cref="SocketOptionName.AddMembership"/>, and <see cref="SocketOptionName.DropMembership"/> options. 
        /// For the <see cref="SocketOptionName.Linger"/> option, use <see cref="Socket"/> for the optionLevel parameter. For <see cref="SocketOptionName.AddMembership"/> and <see cref="SocketOptionName.DropMembership"/>, use <see cref="SocketOptionLevel.IP"/>. If you want to set the value of any of 
        /// the options listed above, use the <see cref="SetSocketOption(SocketOptionLevel, SocketOptionName, Int32)"/> method.
        /// </remarks>
        public object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            if (optionName == SocketOptionName.DontLinger ||
                optionName == SocketOptionName.AddMembership ||
                optionName == SocketOptionName.DropMembership)
            {
                //special case linger?
                throw new NotSupportedException();
            }

            // socket options that don't require any request to the native end
            if(optionLevel == SocketOptionLevel.Socket)
            {
                if(optionName == SocketOptionName.Type)
                {
                    return _socketType;
                }
            }

            // reached here: have to make a request to the lower level to get it

            byte[] val = new byte[4];

            GetSocketOption(optionLevel, optionName, val);

            int iVal = (val[0] << 0) | (val[1] << 8) | (val[2] << 16) | (val[3] << 24);

            return iVal;
        }

        /// <summary>
        /// Returns the specified <see cref="Socket"/> option setting, represented as a byte array.
        /// </summary>
        /// <param name="optionLevel">One of the <see cref="SocketOptionLevel"/> values.</param>
        /// <param name="optionName">One of the <see cref="SocketOptionName"/> values.</param>
        /// <param name="val">An array of type <see cref="Byte"/> that is to receive the option setting.</param>
        /// <remarks>
        /// <para>
        /// <see cref="Socket"/> options determine the behavior of the current <see cref="Socket"/>. Upon successful completion of this method, the array specified by the val parameter contains the value of the specified <see cref="Socket"/> option.
        /// </para>
        /// <para>
        /// When the length of the val array is smaller than the number of bytes required to store the value of the specified <see cref="Socket"/> option, <see cref="GetSocketOption(SocketOptionLevel, SocketOptionName)"/> will 
        /// throw a SocketException. If you receive a <see cref="SocketException"/>, use the <see cref="SocketException.ErrorCode"/> property to obtain the specific error code. After you 
        /// have obtained this code, refer to the Windows Sockets version 2 API error code documentation in the MSDN library for a detailed description of the error. 
        /// Use this overload for any sockets that are represented by Boolean values or integers.
        /// </para>
        /// </remarks>
        public void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] val)
        {
            if (m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            NativeSocket.getsockopt(this, (int)optionLevel, (int)optionName, val);
        }

        /// <summary>
        /// Determines the status of the <see cref="Socket"/>.
        /// </summary>
        /// <param name="microSeconds">The time to wait for a response, in microseconds.</param>
        /// <param name="mode">One of the <see cref="SelectMode"/> values.</param>
        /// <returns>
        /// <para>The status of the Socket based on the polling mode value passed in the mode parameter.</para>
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="SelectMode.SelectRead"/></term>
        /// <description>
        /// <para>true if Listen has been called and a connection is pending;</para>
        /// <para>-or-</para>
        /// <para>true if data is available for reading;</para>
        /// <para>-or-</para>
        /// <para>true if the connection has been closed, reset, or terminated; otherwise, returns false.</para>
        /// </description>
        /// <term><see cref="SelectMode.SelectWrite"/></term>
        /// <description>
        /// <para>true , if processing a Connect, and the connection has succeeded;</para>
        /// <para>-or-</para>
        /// <para>true if data can be sent; otherwise, returns false.</para>
        /// </description>
        /// <term><see cref="SelectMode.SelectError"/></term>
        /// <description>
        /// <para>true if processing a <see cref="Connect"/> that does not block, and the connection has failed;</para>
        /// <para>-or-</para>
        /// <para>true if <see cref="SocketOptionName.OutOfBandInline"/> is not set and out-of-band data is available; otherwise, returns false.</para>
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>
        /// The Poll method will check the state of the <see cref="Socket"/>. Specify <see cref="SelectMode.SelectRead"/> for the selectMode parameter to determine if the <see cref="Socket"/> is readable. 
        /// Specify <see cref="SelectMode.SelectWrite"/> to determine if the <see cref="Socket"/> is writable. Use <see cref="SelectMode.SelectError"/> to detect an error condition. Poll will block execution 
        /// until the specified time period, measured in microseconds, elapses. Set the microSeconds parameter to a negative integer if you would like to wait 
        /// indefinitely for a response. If you want to check the status of multiple sockets, you might prefer to use the Select method.
        /// </para>
        /// <para>
        /// If you receive a <see cref="SocketException"/>, use the <see cref="SocketException.ErrorCode"/> property to obtain the specific error code. After you have obtained this code, refer to the Windows Sockets version 2 API error code documentation in the MSDN library for a detailed description of the error.
        /// </para>
        /// </remarks>
        public bool Poll(int microSeconds, SelectMode mode)
        {
            try
            {
                if (m_Handle == -1)
                {
                    throw new ObjectDisposedException();
                }
                return NativeSocket.poll(this, (int)mode, microSeconds);
            }
            catch
            {
                throw new SocketException(SocketError.SocketError);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Socket"/>, and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        /// <remarks>
        /// <para>This method is called by the public Dispose() method and the <see cref="Finalize"/> method. Dispose() invokes the protected Dispose(Boolean) method with the 
        /// disposing parameter set to true. <see cref="Finalize"/> invokes Dispose with disposing set to false.
        /// </para>
        /// <para>
        /// When the disposing parameter is true, this method releases all resources held by any managed objects that this Socket references. This method invokes the Dispose() method of each referenced object.
        /// </para>
        /// 
        /// </remarks>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected virtual void Dispose(bool disposing)
        {
            if (m_Handle != -1)
            {
                NativeSocket.close(this);
                m_Handle = -1;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~Socket()
        {
            Dispose(false);
        }

        private void Snapshot(ref EndPoint remoteEP)
        {
            IPEndPoint ipSnapshot = remoteEP as IPEndPoint;

            if (ipSnapshot != null)
            {
                ipSnapshot = ipSnapshot.Snapshot();
                // TODO IPv6
                //remoteEP = RemapIPEndPoint(ipSnapshot);

                remoteEP = ipSnapshot;
            }
        }
    }
}
