//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace System.Security.Cryptography
{
    using System;

    /// <summary>
    /// The exception that is thrown when an error occurs during a
    /// cryptographic operation.
    /// </summary>
    [Serializable]
    public class CryptographicException : Exception
    {
        private int _errorCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptographicException"/> class.
        /// </summary>
        public CryptographicException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptographicException"/>
        /// class with a specified error code.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        public CryptographicException(int errorCode)
        {
            _errorCode = errorCode;
        }

        /// <summary>
        /// Gets the error code that is associated with this exception.
        /// </summary>
        public int ErrorCode
        {
            get { return _errorCode; }
        }
    }
}
