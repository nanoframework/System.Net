//
// Copyright (c) 2018 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Sockets
{
    using System;

    /// <summary>
    /// This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.
    /// </summary>
    [Flags]
    public enum SocketFlags
    {
        /// <summary>
        /// Use no flags for this call.
        /// </summary>
        None = 0x0000,
        /// <summary>
        /// Process out-of-band data.
        /// </summary>
        OutOfBand = 0x0001,
        /// <summary>
        ///  Peek at incoming message.
        /// </summary>
        Peek = 0x0002,
        /// <summary>
        ///  Send without using routing tables.
        /// </summary>
        DontRoute = 0x0004,
        /// <summary>
        /// Provides a standard value for the number of WSABUF structures that are used to send and receive data.
        /// </summary>
        MaxIOVectorLength = 0x0010,
        /// <summary>
        /// The message was too large to fit into the specified buffer and was truncated.
        /// </summary>
        Truncated = 0x0100,
        /// <summary>
        /// Indicates that the control data did not fit into an internal 64-KB buffer and was truncated.
        /// </summary>
        ControlDataTruncated = 0x0200,
        /// <summary>
        /// Indicates that the control data did not fit into an internal 64-KB buffer and was truncated.
        /// </summary>
        Broadcast = 0x0400,
        /// <summary>
        /// Indicates a multicast packet.
        /// </summary>
        Multicast = 0x0800,
        /// <summary>
        /// Partial send or receive for message.
        /// </summary>
        Partial = 0x8000,

    }
}
