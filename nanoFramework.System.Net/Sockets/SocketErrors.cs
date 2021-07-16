//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Sockets
{
    /// <summary>
    /// Defines socket error constants.
    /// </summary>
    public enum SocketError : int
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// An unspecified Socket error has occurred.
        /// </summary>
        SocketError = (-1),
 
        /// <summary>
        /// A blocking Socket call was cancelled.
        /// </summary>
        Interrupted = (10000 + 4),      //WSAEINTR
 
        /// <summary>
        /// An attempt was made to access a Socket in a way that is forbidden by its access permissions.
        /// </summary>
        AccessDenied = (10000 + 13),      //WSAEACCES
 
        /// <summary>
        /// An invalid pointer address was detected by the underlying socket provider.
        /// </summary>
        Fault = (10000 + 14),        //WSAEFAULT
 
        /// <summary>
        /// An invalid argument was supplied to a Socket member.
        /// </summary>
        InvalidArgument = (10000 + 22),    //WSAEINVAL

        /// <summary>
        /// There are too many open sockets in the underlying socket provider.
        /// </summary>
        TooManyOpenSockets = (10000 + 24),  //WSAEMFILE

        /// <summary>
        /// An operation on a non-blocking socket cannot be completed immediately.
        /// </summary>
        WouldBlock = (10000 + 35),   //WSAEWOULDBLOCK

        /// <summary>
        /// A blocking operation is in progress.
        /// </summary>
        InProgress = (10000 + 36),  // WSAEINPROGRESS
 
        /// <summary>
        /// The non-blocking Socket already has an operation in progress.
        /// </summary>
        AlreadyInProgress = (10000 + 37),  //WSAEALREADY

        /// <summary>
        /// A Socket operation was attempted on a non-socket.
        /// </summary>
        NotSocket = (10000 + 38),   //WSAENOTSOCK
 
        /// <summary>
        /// A required address was omitted from an operation on a Socket.
        /// </summary>
        DestinationAddressRequired = (10000 + 39), //WSAEDESTADDRREQ
 
        /// <summary>
        /// The datagram is too long.
        /// </summary>
        MessageSize = (10000 + 40),  //WSAEMSGSIZE
 
        /// <summary>
        /// The protocol type is incorrect for this Socket.
        /// </summary>
        ProtocolType = (10000 + 41), //WSAEPROTOTYPE

        /// <summary>
        ///  An unknown, invalid, or unsupported option or level was used with a Socket.
        /// </summary>
        ProtocolOption = (10000 + 42), //WSAENOPROTOOPT

        /// <summary>
        /// The protocol is not implemented or has not been configured.
        /// </summary>
        ProtocolNotSupported = (10000 + 43), //WSAEPROTONOSUPPORT

        /// <summary>
        /// The support for the specified socket type does not exist in this address family.
        /// </summary>
        SocketNotSupported = (10000 + 44), //WSAESOCKTNOSUPPORT

        /// <summary>
        /// The address family is not supported by the protocol family.
        /// </summary>
        OperationNotSupported = (10000 + 45), //WSAEOPNOTSUPP

        /// <summary>
        /// The protocol family is not implemented or has not been configured.
        /// </summary>
        ProtocolFamilyNotSupported = (10000 + 46), //WSAEPFNOSUPPORT

        /// <summary>
        /// The address family specified is not supported. This error is returned if the IPv6 address family was specified and the IPv6 stack is not installed on the local machine. 
        /// This error is returned if the IPv4 address family was specified and the IPv4 stack is not installed on the local machine.
        /// </summary>
        AddressFamilyNotSupported = (10000 + 47), //WSAEAFNOSUPPORT

        /// <summary>
        /// Only one use of an address is normally permitted.
        /// </summary>
        AddressAlreadyInUse = (10000 + 48), // WSAEADDRINUSE
 
        /// <summary>
        /// The selected IP address is not valid in this context.
        /// </summary>
        AddressNotAvailable = (10000 + 49), //WSAEADDRNOTAVAIL

        /// <summary>
        /// The network is not available.
        /// </summary>
        NetworkDown = (10000 + 50), //WSAENETDOWN

        /// <summary>
        /// No route to the remote host exists.
        /// </summary>
        NetworkUnreachable = (10000 + 51), //WSAENETUNREACH

        /// <summary>
        /// The application tried to set KeepAlive on a connection that has already timed out
        /// </summary>
        NetworkReset = (10000 + 52), //WSAENETRESET

        /// <summary>
        /// The connection was aborted by the .NET Framework or the underlying socket provider.
        /// </summary>
        ConnectionAborted = (10000 + 53), //WSAECONNABORTED
 
        /// <summary>
        /// The connection was reset by the remote peer.
        /// </summary>
        ConnectionReset = (10000 + 54), //WSAECONNRESET

        /// <summary>
        /// No free buffer space is available for a Socket operation.
        /// </summary>
        NoBufferSpaceAvailable = (10000 + 55), //WSAENOBUFS

        /// <summary>
        /// The Socket is already connected.
        /// </summary>
        IsConnected = (10000 + 56), //WSAEISCONN

        /// <summary>
        /// The application tried to send or receive data, and the Socket is not connected.
        /// </summary>
        NotConnected = (10000 + 57), //WSAENOTCONN

        /// <summary>
        /// A request to send or receive data was disallowed because the Socket has already been closed.
        /// </summary>
        Shutdown = (10000 + 58), //WSAESHUTDOWN

        /// <summary>
        /// The connection attempt timed out, or the connected host has failed to respond.
        /// </summary>
        TimedOut = (10000 + 60), //WSAETIMEDOUT

        /// <summary>
        /// The remote host is actively refusing a connection.
        /// </summary>
        ConnectionRefused = (10000 + 61), //WSAECONNREFUSED

        /// <summary>
        /// The operation failed because the remote host is down.
        /// </summary>
        HostDown = (10000 + 64), //WSAEHOSTDOWN

        /// <summary>
        /// There is no network route to the specified host.
        /// </summary>
        HostUnreachable = (10000 + 65), //WSAEHOSTUNREACH

        /// <summary>
        /// Too many processes are using the underlying socket provider.
        /// </summary>
        ProcessLimit = (10000 + 67), //WSAEPROCLIM

        /// <summary>
        /// The network subsystem is unavailable.
        /// </summary>
        SystemNotReady = (10000 + 91), //WSASYSNOTREADY
 
        /// <summary>
        /// The version of the underlying socket provider is out of range.
        /// </summary>
        VersionNotSupported = (10000 + 92), //WSAVERNOTSUPPORTED
        ///    <para>
        ///       Successful start-up not yet performed.
        ///    </para>
        /// <summary>
        /// The underlying socket provider has not been initialized.
        /// </summary>
        NotInitialized = (10000 + 93), //WSANOTINITIALISED

        // WSAEREMOTE             = (10000+71),
        /// <devdoc>
        ///    <para>
        ///       Graceful shutdown in progress.
        ///    </para>
        /// </devdoc>
        /// <summary>
        /// A graceful shutdown is in progress.
        /// </summary>
        Disconnecting = (10000 + 101), //WSAEDISCON

        /// <summary>
        /// The specified class was not found.
        /// </summary>
        TypeNotFound = (10000 + 109), //WSATYPE_NOT_FOUND

        /// <summary>
        /// No such host is known. The name is not an official host name or alias.
        /// </summary>
        HostNotFound = (10000 + 1001), //WSAHOST_NOT_FOUND

        /// <summary>
        /// The name of the host could not be resolved. Try again later.
        /// </summary>
        TryAgain = (10000 + 1002), //WSATRY_AGAIN

        /// <summary>
        /// The error is unrecoverable or the requested database cannot be located.
        /// </summary>
        NoRecovery = (10000 + 1003), //WSANO_RECOVERY

        /// <summary>
        /// The requested name or IP address was not found on the name server.
        /// </summary>
        NoData = (10000 + 1004), //WSANO_DATA
    }
}
