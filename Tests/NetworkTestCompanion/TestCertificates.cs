// Copyright (c) .NET Foundation and Contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography.X509Certificates;

namespace NetworkTestCompanion;

/// <summary>
/// Provides the static test certificate used by the companion's TLS echo server.
///
/// This is a self-signed RSA-2048 test certificate (CN=nanoFramework Test Server).
/// The device connects to the companion's TLS echo server with certificate
/// verification disabled, so the certificate only needs to be valid enough for
/// the Windows SChannel server side to complete a handshake.
///
/// The SAME certificate and key are embedded on the device side (SslServerTests.cs)
/// for the reverse scenario (device acting as TLS server). Not a secret — a
/// throw-away test certificate.
/// </summary>
internal static class TestCertificates
{
    private static readonly Lazy<X509Certificate2> _serverCert = new(LoadServerCertificate);

    internal static X509Certificate2 ServerCert => _serverCert.Value;

    private static X509Certificate2 LoadServerCertificate()
    {
        // Load cert + key from PEM, then re-import via PFX so Windows SChannel
        // can use the private key (SChannel rejects the ephemeral key handle
        // that CreateFromPem produces).
        using var fromPem = X509Certificate2.CreateFromPem(ServerCertPem, ServerKeyPem);
        var pfxBytes = fromPem.Export(X509ContentType.Pfx);
        var cert = X509CertificateLoader.LoadPkcs12(pfxBytes, null, X509KeyStorageFlags.Exportable);

        Console.WriteLine("[CERTS] Loaded static test server certificate");
        return cert;
    }

    // Self-signed RSA-2048 test certificate, CN=nanoFramework Test Server.
    // Valid 2026..2036. Throw-away test cert — not a secret.
    internal const string ServerCertPem =
@"-----BEGIN CERTIFICATE-----
MIIDRTCCAi2gAwIBAgIUepRBLWtpFLvLv6rjIIUQxmkVkYkwDQYJKoZIhvcNAQEL
BQAwJDEiMCAGA1UEAwwZbmFub0ZyYW1ld29yayBUZXN0IFNlcnZlcjAeFw0yNjA3
MTYxMjA0MDJaFw0zNjA3MTMxMjA0MDJaMCQxIjAgBgNVBAMMGW5hbm9GcmFtZXdv
cmsgVGVzdCBTZXJ2ZXIwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCu
+yM+X9ZcaawdwfhpJiWa4qlrA/1aV0CoENchMP6XOr4Eq7h/Y8jH+QlKdG2hFe31
wULiwLJq6QwTQ23a7vRFBgTZCZJSs5QY54o2r7O6pO37Y1w+/d0/4blFLNWd0PQq
Mm8TUKdK3J11dv+n/oY9++4vFHR6Bo3xjHFBvm03vcKETeF3UIX+g6J84lfNmdPs
A3UIFqkWXioC7a2+afnRczAHrrS0Py2KcSv+G5E94ZYQHs0VljY8CpOEV2maxh9S
Bjocv4o6HUejKoWvbXqkftuxztjYx77p++jhICpnNZjpNOb27rJhtGw3HPwtn8IY
I3jIZS72insBEQgKSxBhAgMBAAGjbzBtMB0GA1UdDgQWBBR7BiZTDR7gx4gl2fV1
Y9fjrb1mMjAfBgNVHSMEGDAWgBR7BiZTDR7gx4gl2fV1Y9fjrb1mMjAJBgNVHRME
AjAAMAsGA1UdDwQEAwIFoDATBgNVHSUEDDAKBggrBgEFBQcDATANBgkqhkiG9w0B
AQsFAAOCAQEAIeCjTx10sJiacjzajLUJ/dfgSq8VfMUhHQLxG5VF3bPMVbr8LaF+
sM77OKXOBKigDj65urMUwAoLKXF7UcewvVV239Og1p97acetDDiM3Q72duH8MFAF
K2V9qR6Cj8/jvQdSHWiJVOHE0u31PG6Xwa6aIwXA79VKbJaucvj9hXYz0O3HXTRc
RmvHemTge0p0VhuTL+wNv46mftEZhoSsPZa4S08nv5VX3EyWfQM6eX3ghnq6CsE+
MQw94r5CV1kZDA/R9IdXh4aRIVCyN0ZmMvfNNrmIJRdr/eLzQc/6DBeh5Wrg7Sc/
9oUAfyIiPqXG3sW0Txof2L5qbTmallrooQ==
-----END CERTIFICATE-----";

    // PKCS#1 (traditional RSA) encoding — kept identical to the device-side key
    // (SslServerTests.cs), which requires this format for mbedTLS SecureServerInit.
    internal const string ServerKeyPem =
@"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEArvsjPl/WXGmsHcH4aSYlmuKpawP9WldAqBDXITD+lzq+BKu4
f2PIx/kJSnRtoRXt9cFC4sCyaukME0Nt2u70RQYE2QmSUrOUGOeKNq+zuqTt+2Nc
Pv3dP+G5RSzVndD0KjJvE1CnStyddXb/p/6GPfvuLxR0egaN8YxxQb5tN73ChE3h
d1CF/oOifOJXzZnT7AN1CBapFl4qAu2tvmn50XMwB660tD8tinEr/huRPeGWEB7N
FZY2PAqThFdpmsYfUgY6HL+KOh1HoyqFr216pH7bsc7Y2Me+6fvo4SAqZzWY6TTm
9u6yYbRsNxz8LZ/CGCN4yGUu9op7AREICksQYQIDAQABAoIBAB8L2Bj9EB+dcDhn
bhfZ+NoeVUjzkEQzLvmi40i0VLeoaIaToUyY+8rfWNKpDbqDFZGBFMj+v6lQaCAS
2q75rsWAZ+PKWvfpfOFeU5uYWR9InCD6ZCeZC2SGPEUVy2EQ7gF+qU6YBNa3hgiN
cJbyBgeBZ6Vaz7/G4fB1prKvgtlcunjtXAwdme9nkHR2kuG+pGGtNs/qc71bQeOt
5gphHdls+lHX+D6QD/gB2biR1bSJW+Cegz0zNM0nUUz6cA3K9dUTMeyGyI4c8FNt
u/FuRZlri/I7yUAPngVnmq46rJg1Ih0OvXI+jIEHp7SI0e9MiRLnZhB4x3/76940
qAJ+nnECgYEA5692QqZ0eovLlqlWSrRNP0mG31HxCTrWYvfvfWkfhGPskdsJoOeQ
RXk5Mvfp84miIezv8aXxmQEquxQiP8HRtzQ0TrsvKdlM9XOGwUJULyqtFfSs6vmj
HTD16GGPxWqEy0xAcaQcM31pb8YwuZ5MCNRdhuL4ladWBfMR0z75GssCgYEAwVg8
hPsX+H4LzCfu32MQCMh+sAR+1xPTLSG3ydJFbx6PE9pX408ZJKc9CtkG8Xz29+y1
EZnymB1IUpNZxms/4pybyFaKXUa20SpPgSoIrBaL/wdgM4h4Pvs2FSceYN9qY7Z5
d4DhJEiuez+CAFVqQnrNaLJI7xD094SEv91nQAMCgYA+rq8dOzG6UgYj3e61yXA4
1ijCVMYUzDFil1fZI07en7ZKg+tn+B6FXVXHX2GRfUQ7T4Jfa5kg3zrzYHAftc2K
dnpMbsJE3UDAC6CCuvJRzIcFsKvz6tRhunRdib+/FqGU6y1oUZE7sQuMrR9TqOtD
XEltjAzbWGmitG+3Kot03wKBgQCunriaCgWOQpj5HB/b1aaHqDzzUDwWmCskGc3a
E3TudRUYAx1ZiPjWZ8zz3SsuM4UCSeEHMpkt1VSab8anM/oQ+wyflbmFoPZAVwxT
RdlrQznRbaHvKRQhHdWsqRYAvAdkY0u1KMsucA5V9fe9wWck/7BBHLROZmw4mJEk
kBxObQKBgQCCrcUNFhm3dxCdi+VgMwrhMOqiO5XYAGw4raQ/BbXwpb1PLbj4xtSZ
WVb3utTBP0WPhf58EcHc8ko4B+1xCMR4B9rntQACfngbUN4wETQ1Gz+bNgaHQyJo
gHYA38gnEOJurr2VLZFaqLgwj+7kpTRL2a0ZTDz8pCxlwDpsi4n1YA==
-----END RSA PRIVATE KEY-----";
}
