//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// Provides methods that help you use X.509 v.3 certificates.
    /// </summary>
    /// <remarks>
    /// Supported formats: DER and PEM.
    /// </remarks>
    public class X509Certificate
    {
        private readonly byte[] _certificate;

        /// <summary>
        /// Contains the certificate issuer.
        /// </summary>
        protected string _issuer;
        /// <summary>
        /// Contains the subject.
        /// </summary>
        protected string _subject;
        /// <summary>
        /// Contains the effective date of the certificate.
        /// </summary>
        protected DateTime _effectiveDate;
        /// <summary>
        /// Contains the expiration date of the certificate.
        /// </summary>
        protected DateTime _expirationDate;
        /// <summary>
        /// Contains the handle.
        /// </summary>
        protected byte[] _handle;
        /// <summary>
        /// Contains the session handle.
        /// </summary>
        protected byte[] _sessionHandle;

        /// <summary>
        /// Initializes a new instance of the X509Certificate class.
        /// </summary>
        public X509Certificate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate"/> class defined from a sequence of bytes representing an X.509v3 certificate.
        /// </summary>
        /// <param name="certificate">A byte array containing data from an X.509 certificate.</param>
        /// <remarks>
        /// DER and PEM encoding are the supported formats. 
        /// </remarks>
        public X509Certificate(byte[] certificate)
        {
            _certificate = certificate;

            ParseCertificate(
                certificate,
                ref _issuer,
                ref _subject,
                ref _effectiveDate,
                ref _expirationDate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="X509Certificate"/> class defined from a string with the content of an X.509v3 certificate.
        /// </summary>
        /// <param name="certificate">A string containing a X.509 certificate.</param>
        /// <remarks>
        /// Supported formats: DER and PEM.
        /// This methods is exclusive of .NET nanoFramework. The equivalent .NET constructor accepts a file name as the parameter.
        /// </remarks>
        public X509Certificate(string certificate)
        {
            var tempCertificate = Encoding.UTF8.GetBytes(certificate);

            //////////////////////////////////////////////
            // because this is parsing from a string    //
            // we need to keep the terminator           //
            //////////////////////////////////////////////
            _certificate = new byte[tempCertificate.Length + 1];
            Array.Copy(tempCertificate, _certificate, tempCertificate.Length);
            _certificate[_certificate.Length - 1] = 0;

            ParseCertificate(
                _certificate,
                ref _issuer,
                ref _subject,
                ref _effectiveDate,
                ref _expirationDate);
        }

        /// <summary>
        /// Gets the name of the certificate authority that issued the X.509v3 certificate.
        /// </summary>
        /// <value>
        /// The name of the certificate authority that issued the X.509v3 certificate.
        /// </value>
        public virtual string Issuer
        {
            get { return _issuer; }
        }

        /// <summary>
        /// Gets the subject distinguished name from the certificate.
        /// </summary>
        /// <value>
        /// The subject distinguished name from the certificate.
        /// </value>
        public virtual string Subject
        {
            get { return _subject; }
        }

        /// <summary>
        /// Returns the effective date of this X.509v3 certificate.
        /// </summary>
        /// <returns>The effective date for this X.509 certificate.</returns>
        /// <remarks>
        /// This methods is exclusive of nanoFramework. The equivalent .NET method is GetEffectiveDateString().
        /// </remarks>
        public virtual DateTime GetEffectiveDate()
        {
            return _effectiveDate;
        }

        /// <summary>
        /// Returns the expiration date of this X.509v3 certificate.
        /// </summary>
        /// <returns>The expiration date for this X.509 certificate.</returns>
        /// <remarks>
        /// This methods is exclusive of nanoFramework. The equivalent .NET method is GetExpirationDateString().
        /// </remarks>
        public virtual DateTime GetExpirationDate()
        {
            return _expirationDate;
        }

        /// <summary>
        /// Returns the raw data for the entire X.509v3 certificate as an array of bytes.
        /// </summary>
        /// <returns>A byte array containing the X.509 certificate data.</returns>
        public virtual byte[] GetRawCertData()
        {
            return _certificate;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void ParseCertificate(
            byte[] cert,
            ref string issuer,
            ref string subject,
            ref DateTime effectiveDate,
            ref DateTime expirationDate);
    }
}

