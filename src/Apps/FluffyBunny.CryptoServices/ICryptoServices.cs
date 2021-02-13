using System;

namespace FluffyBunny.CryptoServices
{
    public interface ICryptoServices
    {
        /// <summary>
        /// Creates a RSA certificate
        /// </summary>
        /// <param name="dnsName i.e. localhost"></param>
        /// <param name="validFrom"></param>
        /// <param name="validTo"></param>
        /// <param name="password"></param>
        /// <returns>A Base64 PFX</returns>
        string CreateRSACertificatePFX(string dnsName, DateTimeOffset validFrom, DateTimeOffset validTo,string password);
        /// <summary>
        /// Creates an ECDsa Certificate
        /// </summary>
        /// <param name="dnsName i.e. localhost"></param>
        /// <param name="validFrom"></param>
        /// <param name="validTo"></param>
        /// <param name="password"></param>
        /// <returns>A Base64 PFX</returns>
        string CreateECDsaCertificatePFX(string dnsName, DateTimeOffset validFrom, DateTimeOffset validTo, string password);
    }
}