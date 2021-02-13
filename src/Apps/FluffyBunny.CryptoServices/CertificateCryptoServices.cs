using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CertificateManager;
using CertificateManager.Models;

namespace FluffyBunny.CryptoServices
{
    public class CertificateCryptoServices : ICryptoServices
    {
        private CreateCertificates _createCertificates;
        private ImportExportCertificate _importExportCertificate;

        public CertificateCryptoServices(CreateCertificates createCertificates, ImportExportCertificate importExportCertificate)
        {
            _createCertificates = createCertificates;
            _importExportCertificate = importExportCertificate;
        }
        public string CreateECDsaCertificatePFX(string dnsName, DateTimeOffset validFrom, DateTimeOffset validTo, string password)
        {
            var basicConstraints = new BasicConstraints
            {
                CertificateAuthority = false,
                HasPathLengthConstraint = false,
                PathLengthConstraint = 0,
                Critical = false
            };

            var san = new SubjectAlternativeName
            {
                DnsName = new List<string> { dnsName }
            };

            var x509KeyUsageFlags = X509KeyUsageFlags.DigitalSignature;

            // only if certification authentication is used
            var enhancedKeyUsages = new OidCollection {
                new Oid("1.3.6.1.5.5.7.3.1"),  // TLS Server auth
                new Oid("1.3.6.1.5.5.7.3.2"),  // TLS Client auth
            };
            
            var certificate = _createCertificates.NewECDsaSelfSignedCertificate(
                new DistinguishedName { CommonName = dnsName },
                basicConstraints,
                new ValidityPeriod
                {
                    ValidFrom = validFrom,
                    ValidTo = validTo
                },
                san,
                enhancedKeyUsages,
                x509KeyUsageFlags,
                new ECDsaConfiguration()
                {
                    
                });
            var ecdsaCertPfxBytes = _importExportCertificate.ExportSelfSignedCertificatePfx(password, certificate);

            var pfxBase64 = Convert.ToBase64String(ecdsaCertPfxBytes);
            return pfxBase64;
        }

        public string CreateRSACertificatePFX(string dnsName, DateTimeOffset validFrom, DateTimeOffset validTo, string password)
        {
            var basicConstraints = new BasicConstraints
            {
                CertificateAuthority = false,
                HasPathLengthConstraint = false,
                PathLengthConstraint = 0,
                Critical = false
            };

            var subjectAlternativeName = new SubjectAlternativeName
            {
                DnsName = new List<string> { dnsName }
            };

            var x509KeyUsageFlags = X509KeyUsageFlags.DigitalSignature;

            // only if certification authentication is used
            var enhancedKeyUsages = new OidCollection
            {
                new Oid("1.3.6.1.5.5.7.3.1"),  // TLS Server auth
                new Oid("1.3.6.1.5.5.7.3.2"),  // TLS Client auth
            };

           

            var certificate = _createCertificates.NewRsaSelfSignedCertificate(
                new DistinguishedName { CommonName = dnsName },
                basicConstraints,
                new ValidityPeriod
                {
                    ValidFrom = validFrom,
                    ValidTo = validTo
                },
                subjectAlternativeName,
                enhancedKeyUsages,
                x509KeyUsageFlags,
                new RsaConfiguration { KeySize = (int)2048 }
            );
            var rsaCertPfxBytes = _importExportCertificate.ExportSelfSignedCertificatePfx(password, certificate);
            var pfxBase64 = Convert.ToBase64String(rsaCertPfxBytes);
            return pfxBase64;
        }
    }
}
