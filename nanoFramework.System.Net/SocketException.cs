//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Net.Sockets
{
    using System;

    /// <summary>
    /// The exception that is thrown when a socket error occurs.
    /// </summary>
    [Serializable]
    public class SocketException : Exception
    {
        private int _errorCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public SocketException(SocketError errorCode)
        {
            _errorCode = (int)errorCode;
        }

        /// <summary>
        /// Gets the error code that is associated with this exception.
        /// </summary>
        /// <remarks>
        /// <para>The ErrorCode property contains the error code that is associated with the error that caused the exception.</para>
        /// <para>The default constructor for <see cref="SocketException"/> sets the ErrorCode property to the last operating system error that occurred. For more information about socket error codes, see the Windows Sockets version 2 API error code documentation in MSDN.</para>
        /// </remarks>
        public int ErrorCode
        {
            get { return _errorCode; }
        }
    }
}
