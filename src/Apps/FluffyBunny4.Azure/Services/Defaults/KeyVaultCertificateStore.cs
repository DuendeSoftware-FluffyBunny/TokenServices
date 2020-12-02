using Azure.Security.KeyVault.Keys;
using FluffyBunny4.Configuration;
using FluffyBunny4.Extensions;
using FluffyBunny4.Models;
 
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace FluffyBunny4.Services
{
    public class KeyVaultCertificateStore : IKeyVaultCertificateStore, IJwksDiscovery
    {
        private IConfiguration _configuration;
        private KeyVaultStoreOptions _options;
        private CacheData _cacheData;
        private List<Duende.IdentityServer.Models.JsonWebKey> _jwkRecords;
        private DateTime _lastRead;

        public KeyVaultCertificateStore(
            IConfiguration configuration,
            IOptions<KeyVaultStoreOptions> options)
        {
            _configuration = configuration;
            _options = options.Value;
        }

        private async Task<List<KeyBundle>> GetKeyBundleVersionsAsync(KeyVaultClient keyVaultClient)
        {
            List<KeyItem> keyItems = new List<KeyItem>();

            var page = await keyVaultClient.GetKeyVersionsAsync(_options.KeyVaultUrl, _options.KeyIdentifier);
            keyItems.AddRange(page);
            while (!string.IsNullOrWhiteSpace(page.NextPageLink))
            {
                page = await keyVaultClient.GetKeyVersionsNextAsync(page.NextPageLink);
                keyItems.AddRange(page);

            }
            var keyBundles = new List<KeyBundle>();

            foreach (var keyItem in keyItems)
            {
                var keyBundle = await keyVaultClient.GetKeyAsync(keyItem.Identifier.Identifier);
                keyBundles.Add(keyBundle);
            }

            return keyBundles;
        }
        private KeyBundle GetLatestKeyBundleWithRolloverDelay(List<KeyBundle> kbs)
        {
            // First limit the search to just those cerutificates that have existed longer than the rollover delay.
            var rolloverCutoff = DateTime.UtcNow.Subtract(new TimeSpan(720, 0, 0));
            var potentialCerts = kbs.Where(c => c.Attributes.NotBefore < rolloverCutoff);

            // If no certs could be found, then widen the search to any usable certificate.
            if (!potentialCerts.Any())
            {
                potentialCerts = kbs.Where(c => c.Attributes.NotBefore < DateTime.UtcNow);
            }

            // Of the potential certs, return the newest one.
            return potentialCerts
                .OrderByDescending(c => c.Attributes.NotBefore)
                .FirstOrDefault();
        }
        private X509Certificate2 GetLatestCertificateWithRolloverDelay(List<X509Certificate2> certificates)
        {
            // First limit the search to just those certificates that have existed longer than the rollover delay.
            var rolloverCutoff = DateTime.UtcNow.AddHours(-720);
            var potentialCerts = certificates.Where(c => c.NotBefore < rolloverCutoff);

            // If no certs could be found, then widen the search to any usable certificate.
            if (!potentialCerts.Any())
            {
                potentialCerts = certificates.Where(c => c.NotBefore < DateTime.UtcNow);
            }

            // Of the potential certs, return the newest one.
            return potentialCerts
                .OrderByDescending(c => c.NotBefore)
                .FirstOrDefault();
        }

        private async Task<List<X509Certificate2>> GetAllCertificateVersions(KeyVaultClient keyVaultClient)
        {


            var certificates = new List<X509Certificate2>();

            // Get the first page of certificates
            var certificateItemsPage = await keyVaultClient.GetCertificateVersionsAsync(_options.KeyVaultUrl, _options.KeyIdentifier);
            while (true)
            {
                foreach (var certificateItem in certificateItemsPage)
                {
                    // Ignored disabled or expired certificates
                    if (certificateItem.Attributes.Enabled == true &&
                        (certificateItem.Attributes.Expires == null || certificateItem.Attributes.Expires > DateTime.UtcNow))
                    {
                        var certificateVersionBundle = await keyVaultClient.GetCertificateAsync(certificateItem.Identifier.Identifier);
                        var certificatePrivateKeySecretBundle = await keyVaultClient.GetSecretAsync(certificateVersionBundle.SecretIdentifier.Identifier);
                        var privateKeyBytes = Convert.FromBase64String(certificatePrivateKeySecretBundle.Value);
                        var certificateWithPrivateKey = new X509Certificate2(privateKeyBytes, (string)null, X509KeyStorageFlags.MachineKeySet);

                        certificates.Add(certificateWithPrivateKey);
                    }
                }

                if (certificateItemsPage.NextPageLink == null)
                {
                    break;
                }
                else
                {
                    // Get the next page
                    certificateItemsPage = await keyVaultClient.GetCertificateVersionsNextAsync(certificateItemsPage.NextPageLink);
                }
            }

            return certificates;
        }
        string StripPort(string url)
        {
            UriBuilder u1 = new UriBuilder(url);
            u1.Port = -1;
            string clean = u1.Uri.ToString();

            return clean;
        }
        async Task HardFetchJwkRecordsAsync()
        {
            _jwkRecords = new List< Duende.IdentityServer.Models.JsonWebKey > ();
            try
            {
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
                KeyVaultClient keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));


                var keyBundles = await GetKeyBundleVersionsAsync(keyVaultClient);
                var queryKbs = from item in keyBundles
                               where item.Attributes.Enabled != null && (bool)item.Attributes.Enabled
                               && (item.Attributes.Expires == null || item.Attributes.Expires > DateTime.UtcNow)
                               select item;
                keyBundles = queryKbs.ToList();
                var latestKB = GetLatestKeyBundleWithRolloverDelay(keyBundles);

                X509Certificate2 x509Certificate2 = null;

                var x509Certificate2s = await GetAllCertificateVersions(keyVaultClient);
                x509Certificate2 = GetLatestCertificateWithRolloverDelay(x509Certificate2s);


                var queryRsaSecurityKeys = from item in keyBundles
                                           let c = new RsaSecurityKey(item.Key.ToRSA())
                                           {
                                               KeyId = StripPort(item.KeyIdentifier.Identifier)
                                           }
                                           select c;

                var jwks = new List<JsonWebKey>();
                foreach (var keyBundle in keyBundles)
                {
                    jwks.Add(new JsonWebKey(keyBundle.Key.ToString()));
                }

                var jwk = latestKB.Key;
                var kid = latestKB.KeyIdentifier;

                var parameters = new RSAParameters
                {
                    Exponent = jwk.E,
                    Modulus = jwk.N
                };
                var securityKey = new RsaSecurityKey(parameters)
                {
                    KeyId = jwk.Kid.LongKidStringToHash(),
                };

                SigningCredentials signingCredentials;

                signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);



                _cacheData = new CacheData()
                {
                    RsaSecurityKeys = queryRsaSecurityKeys.ToList(),
                    SigningCredentials = signingCredentials,
                    JsonWebKeys = jwks,
                    KeyIdentifier = kid,
                    X509Certificate2 = x509Certificate2
                };
                foreach (var item in _cacheData.JsonWebKeys)
                {
                    var idx = item.KeyId.LastIndexOf('/');
                    var kid2 = item.KeyId.Substring(idx + 1);
                    _jwkRecords.Add(new Duende.IdentityServer.Models.JsonWebKey
                    {
                        alg = "RS256",
                        use = "sig",
                        e = item.E,
                        kty = item.Kty,
                        n = item.N,
                        kid = kid2

                    });
                }
            }
            catch (Exception ex)
            {
                _jwkRecords = null;
            }
        }
        public async Task<JwksDiscoveryDocument> FetchJwkRecordsAsync()
        {

            if (_jwkRecords == null)
            {
                await HardFetchJwkRecordsAsync();
                _lastRead = DateTime.UtcNow;
            }
            else
            {
                TimeSpan diff = DateTime.UtcNow.Subtract(_lastRead);
                if (diff.TotalSeconds > _options.ExpirationSeconds)
                {
                    await HardFetchJwkRecordsAsync();
                    _lastRead = DateTime.UtcNow;
                }
            }
            return new JwksDiscoveryDocument { Keys = _jwkRecords };
        }

        public async Task<CacheData> FetchCacheDataAsync()
        {
            if (_cacheData == null)
            {
                await HardFetchJwkRecordsAsync();
                _lastRead = DateTime.UtcNow;
            }
            else
            {
                TimeSpan diff = DateTime.UtcNow.Subtract(_lastRead);
                if (diff.TotalSeconds > _options.ExpirationSeconds)
                {
                    await HardFetchJwkRecordsAsync();
                    _lastRead = DateTime.UtcNow;
                }
            }
            return _cacheData;
        }
    }
}
