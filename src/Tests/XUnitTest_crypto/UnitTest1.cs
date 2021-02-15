using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CertificateManager;
using CertificateManager.Models;
using Duende.IdentityServer;
using Duende.IdentityServer.Configuration;
using FluffyBunny.CryptoServices;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Duende.IdentityServer.Models;

namespace XUnitTest_crypto
{
    public class UnitTest1
    {
        internal static string GetECDsaSigningAlgorithmValue(IdentityServerConstants.ECDsaSigningAlgorithm value)
        {
            return value switch
            {
                IdentityServerConstants.ECDsaSigningAlgorithm.ES256 => SecurityAlgorithms.EcdsaSha256,
                IdentityServerConstants.ECDsaSigningAlgorithm.ES384 => SecurityAlgorithms.EcdsaSha384,
                IdentityServerConstants.ECDsaSigningAlgorithm.ES512 => SecurityAlgorithms.EcdsaSha512,
                _ => throw new ArgumentException("Invalid ECDsa signing algorithm value", nameof(value)),
            };
        }
        private static CreateCertificates _cc;
        [Fact]
        public void Test1()
        {
            var sp = new ServiceCollection()
                .AddCertificateManager()
                .AddSingleton< ICryptoServices,CertificateCryptoServices> ()
                .BuildServiceProvider();

            string password = "1234";
            string dnsName = "localhost";
            var cryptoServices = sp.GetRequiredService<ICryptoServices>();
            var ecdsaPFX = cryptoServices.CreateECDsaCertificatePFX( dnsName,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1), password);
            var rsaPFX = cryptoServices.CreateRSACertificatePFX( dnsName,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1), password);

            _cc = sp.GetService<CreateCertificates>();

            byte[] ecdsaCertPfxBytes = Convert.FromBase64String(ecdsaPFX);
            var ecdsaCertificate = new X509Certificate2(ecdsaCertPfxBytes, password);


            byte[] rsaCertPfxBytes = Convert.FromBase64String(rsaPFX);
            var rsaCertificate = new X509Certificate2(rsaCertPfxBytes, password);

            ECDsaSecurityKey ecdsaCertificatePublicKey = new ECDsaSecurityKey(ecdsaCertificate.GetECDsaPrivateKey());

            var credential = new SigningCredentials(ecdsaCertificatePublicKey,
                GetECDsaSigningAlgorithmValue(IdentityServerConstants.ECDsaSigningAlgorithm.ES256));
            var keyInfo = new SecurityKeyInfo
            {
                Key = credential.Key,
                SigningAlgorithm = credential.Algorithm
            };







        }
       
    }
}
