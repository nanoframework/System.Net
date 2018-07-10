////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace System.Net
{
    /// <summary>
    ///  Identifies a network address. This is an abstract class.
    /// </summary>
    /// <remarks>
    /// The EndPoint class provides an abstract base class that represents a network resource or service. Descendant classes 
    /// combine network connection information to form a connection point to a service.
    /// </remarks>
    [Serializable]
    public abstract class EndPoint
    {
        /// <summary>
        /// Serializes endpoint information into a <see cref="SocketAddress"/> instance.
        /// </summary>
        /// <returns>
        /// A <see cref="SocketAddress"/> instance that contains the endpoint information.
        /// </returns>
        public abstract SocketAddress Serialize();
        /// <summary>
        /// Creates an <see cref="EndPoint"/> instance from a <see cref="SocketAddress"/> instance.
        /// </summary>
        /// <param name="socketAddress">The socket address that serves as the endpoint for a connection.</param>
        /// <returns>
        /// A new <see cref="EndPoint"/> instance that is initialized from the specified <see cref="SocketAddress"/> instance.
        /// </returns>
        public abstract EndPoint Create(SocketAddress socketAddress);

    }; // abstract class EndPoint

} // namespace System.Net


