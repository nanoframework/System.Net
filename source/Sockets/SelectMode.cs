//
// Copyright (c) 2018 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Sockets
{
    /// <summary>
    /// Specifies the mode for polling the status of a socket.
    /// </summary>
    public enum SelectMode
    {
        /// <summary>
        /// Poll the read status of a socket.
        /// </summary>
        SelectRead = 0,
        /// <summary>
        /// Poll the write status of a socket.
        /// </summary>
        SelectWrite = 1,
        /// <summary>
        /// Poll the error status of a socket.
        /// </summary>
        SelectError = 2
    }
}
