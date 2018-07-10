////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Security.Cryptography.X509Certificates 
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides methods that help you use X.509 v.3 certificates.
    /// </summary>
    public class X509Certificate
    {
        private byte[] m_certificate;
        private string m_password;

        /// <summary>
        /// Contains the certificate issuer.
        /// </summary>
        protected string m_issuer;
        /// <summary>
        /// Contains the subject.
        /// </summary>
        protected string m_subject;
        /// <summary>
        /// Contains the effective date of the certificate.
        /// </summary>
        protected DateTime m_effectiveDate;
        /// <summary>
        /// Contains the expiration date of the certificate.
        /// </summary>
        protected DateTime m_expirationDate;
        /// <summary>
        /// Contains the handle.
        /// </summary>
        protected byte[] m_handle;
        /// <summary>
        /// Contains the session handle.
        /// </summary>
        protected byte[] m_sessionHandle;

        /// <summary>
        /// Initializes a new instance of the X509Certificate class.
        /// </summary>
        public X509Certificate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the X509Certificate class defined from a sequence of bytes representing an X.509v3 certificate.
        /// </summary>
        /// <param name="certificate">
        /// certificate
        /// Type: Byte[] - A byte array containing data from an X.509 certificate.
        /// </param>
        public X509Certificate(byte[] certificate)
            : this(certificate, "")
        {
        }

        /// <summary>
        /// Initializes a new instance of the X509Certificate class using a byte array and a password.
        /// </summary>
        /// <param name="certificate">
        /// certificate
        /// Type: Byte[] - A byte array containing data from an X.509 certificate.
        /// </param>
        /// <param name="password">
        /// Type:String - The password required to access the X.509 certificate data.
        /// </param>
        public X509Certificate(byte[] certificate, string password)
        {
            m_certificate = certificate;
            m_password    = password;

            ParseCertificate(certificate, password, ref m_issuer, ref m_subject, ref m_effectiveDate, ref m_expirationDate);
        }

        /// <summary>
        /// Gets the name of the certificate authority that issued the X.509v3 certificate.
        /// </summary>
        public virtual string Issuer
        {
            get { return m_issuer; }
        }

        /// <summary>
        /// Gets the subject distinguished name from the certificate.
        /// </summary>
        public virtual string Subject
        {
            get { return m_subject; }
        }

        /// <summary>
        /// Gets the effective date of the certificate.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetEffectiveDate()
        {
            return m_effectiveDate;
        }

        /// <summary>
        /// Gets the expiration date of the certificate.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetExpirationDate()
        {
            return m_expirationDate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetRawCertData()
        {
            return m_certificate;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ParseCertificate(byte[] cert, string password, ref string issuer, ref string subject, ref DateTime effectiveDate, ref DateTime expirationDate);
    }
}

