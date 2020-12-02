using Azure.Security.KeyVault.Keys;
using FluffyBunny4.Azure.Clients;
using FluffyBunny4.Configuration;
using FluffyBunny4.DotNetCore;
using FluffyBunny4.Models;
using FluffyBunny4.Services;
using IdentityModel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Microsoft.Extensions.Logging;
using static FluffyBunny4.DotNetCore.TimedLock;

namespace FluffyBunny4.Stores
{
    public class KeyVaultECDsaKeyStore :
        IKeyVaultECDsaKeyStoreConfiguration,
        IJwksDiscovery,
        IKeyVaultECDsaKeyStore,
        IValidationKeysStore
    {
        internal static class Constants
        {
            public static class CurveOids
            {
                public const string P256 = "1.2.840.10045.3.1.7";
                public const string P384 = "1.3.132.0.34";
                public const string P521 = "1.3.132.0.35";
            }
        }
        private IConfiguration _configuration;
        private IAzureKeyVaultClients _azureKeyVaultClients;
        private IMemoryCache _memoryCache;
        private KeyVaultStoreOptions _options;
        private TimedLock _lock;
        private ILogger<KeyVaultECDsaKeyStore> _logger;
        private const string CacheKey = "3bac4de0-ae68-4dd9-1111-4e0b92558426";
        private const string ECDsaKeyCacheKey = "3bac4de0-ae68-4dd9-2222-4e0b92558426";

        public KeyVaultECDsaKeyStore(
              IConfiguration configuration,
              IAzureKeyVaultClients azureKeyVaultClients,
              IMemoryCache memoryCache,
              IOptions<KeyVaultStoreOptions> options,
              ILogger<KeyVaultECDsaKeyStore> logger)
        {
            _configuration = configuration;
            _azureKeyVaultClients = azureKeyVaultClients;
            _memoryCache = memoryCache;
            _options = options.Value;
            _lock = new TimedLock();
            _logger = logger;
        }

        public async Task<JwksDiscoveryDocument> FetchJwkRecordsAsync()
        {
            return await GetECDsaJwksDiscoveryDocumentAsync(_options.KeyVaultName, _options.KeyIdentifier);
        }
        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            var ecdsaCache = await FetchCacheAsync();
            return ecdsaCache.SecurityKeyInfos;

        }
        public async Task<JwksDiscoveryDocument> GetECDsaJwksDiscoveryDocumentAsync(string keyVaultName, string keyName)
        {
            var result = new List<Duende.IdentityServer.Models.JsonWebKey>();
                 
            var ecdsaCache = await FetchCacheAsync();
            foreach (var keyInfo in ecdsaCache.SecurityKeyInfos)
            {
                var ecdsaKey = keyInfo.Key as ECDsaSecurityKey;

                var parameters = ecdsaKey.ECDsa.ExportParameters(false);
                var x = Base64Url.Encode(parameters.Q.X);
                var y = Base64Url.Encode(parameters.Q.Y);

                var ecdsaJsonWebKey = new Duende.IdentityServer.Models.JsonWebKey
                {
                    kty = "EC",
                    use = "sig",
                    kid = ecdsaKey.KeyId,
                    x = x,
                    y = y,
                    crv = GetCrvValueFromCurve(parameters.Curve),
                    alg = keyInfo.SigningAlgorithm
                };
                result.Add(ecdsaJsonWebKey);
            }

            return new JwksDiscoveryDocument()
            {
                Keys = result
            };

        }
        internal static string GetCrvValueFromCurve(ECCurve curve)
        {
            return curve.Oid.Value switch
            {
                Constants.CurveOids.P256 => JsonWebKeyECTypes.P256,
                Constants.CurveOids.P384 => JsonWebKeyECTypes.P384,
                Constants.CurveOids.P521 => JsonWebKeyECTypes.P521,
                _ => throw new InvalidOperationException($"Unsupported curve type of {curve.Oid.Value} - {curve.Oid.FriendlyName}"),
            };
        }
        private async Task<(List<KeyProperties>, List<SecurityKeyInfo>, KeyProperties)> FetchWorkingKeyPropertiesAsync()
        {
            var utcNow = DateTime.UtcNow;
            DateTimeOffset? currentStartOn = utcNow.Subtract(new TimeSpan(10000, 0, 0, 0));

            var keyClient = _azureKeyVaultClients.CreateKeyClient(_options.KeyVaultUrl);
            var propertiesOfKeyVersionsAsync = keyClient.GetPropertiesOfKeyVersionsAsync(_options.KeyIdentifier);
            var pages = propertiesOfKeyVersionsAsync.AsPages();
            var securityKeyInfos = new List<SecurityKeyInfo>();
            

            var finalKeyProperties = new List<KeyProperties>();
            KeyProperties currentKeyProperties = null;
            await foreach (var page in pages)
            {
                var keyProperties = page.Values;
                foreach (var prop in keyProperties)
                {
                    if ((bool)!prop.Enabled)
                    {
                        continue;
                    }
                    if ((bool)prop.Enabled && utcNow < prop.ExpiresOn)
                    {
                        finalKeyProperties.Add(prop);

                        var key = await keyClient.GetKeyAsync(prop.Name, prop.Version);

                        var ecDsa = key.Value.Key.ToECDsa();
                        var securityKey = new ECDsaSecurityKey(ecDsa) { KeyId = prop.Version };
                        var algorithm = "";

                        if (key.Value.Key.CurveName == KeyCurveName.P256)
                        {
                            algorithm = "ES256";
                        }
                        else if (key.Value.Key.CurveName == KeyCurveName.P384)
                        {
                            algorithm = "ES384";
                        }
                        else if (key.Value.Key.CurveName == KeyCurveName.P521)
                        {
                            algorithm = "ES521";
                        }
                        else
                        {
                            continue;
                        }
                        securityKeyInfos.Add(new SecurityKeyInfo
                        {
                            Key = securityKey,
                            SigningAlgorithm = algorithm
                        });

                        if (prop.NotBefore <= utcNow)
                        {
                            if (prop.NotBefore > currentStartOn)
                            {
                                currentStartOn = prop.NotBefore;
                                currentKeyProperties = prop;
                            }
                        }
                    }
                }
            }
            finalKeyProperties = finalKeyProperties.OrderBy(x => x.ExpiresOn).ToList(); // ToList optional

            return (finalKeyProperties, securityKeyInfos,currentKeyProperties);

        }
        public async Task<ECDsaKeyCache> FetchCacheAsync()
        {
            LockReleaser releaser = await _lock.Lock(new TimeSpan(0, 0, 30));
            try
            {
                var cacheEntry = await _memoryCache.GetOrCreate($"{_options.KeyIdentifier}-{ECDsaKeyCacheKey}", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

                    var (finalKeyProperties, securityKeyInfos, currentKeyProperties) = await FetchWorkingKeyPropertiesAsync();
     
                    if (currentKeyProperties == null)
                    {
                        _logger.LogError($"No available ECDsa Keys, attempting to create 2");
                        // lets create a couple.
                        await CreateKeysAsync(DateTime.UtcNow.AddDays(-1),2, 9 * 30, 3 * 30);
                        (finalKeyProperties, securityKeyInfos, currentKeyProperties) = await FetchWorkingKeyPropertiesAsync();
                    }
                    if (currentKeyProperties == null)
                    {
                        _logger.LogError($"failed to create ECDsa Keys");
                        throw new Exception("Unknown issue with keys");
                    }

                    if (finalKeyProperties.Count <= 2)
                    {
                        _logger.LogError($"We only have 2 ECDsa keys in reserve,attempting to create an extra one.");
                        // add some more keys in the future.
                        var lastEntry = finalKeyProperties.LastOrDefault();
                        DateTime utcStartTime = ((DateTimeOffset)lastEntry.ExpiresOn).DateTime;
                        utcStartTime = utcStartTime.Subtract(new TimeSpan(3 * 30, 0, 0, 0));
                        await CreateKeysAsync(utcStartTime, 1, 9 * 30, 3 * 30);
                        (finalKeyProperties, securityKeyInfos, currentKeyProperties) = await FetchWorkingKeyPropertiesAsync();

                        if (finalKeyProperties.Count <= 2)
                        {
                            _logger.LogError($"failed to create ECDsa Keys");
                        }

                    }

                    var keyClient = _azureKeyVaultClients.CreateKeyClient(_options.KeyVaultUrl);
                    var cryptoClient = await _azureKeyVaultClients.CreateCryptographyClientAsync(keyClient, _options.KeyIdentifier, currentKeyProperties.Version);

                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12);
                    return new ECDsaKeyCache
                    {
                        CurrentKeyProperties = currentKeyProperties,
                        CryptographyClient = cryptoClient,
                        SecurityKeyInfos = securityKeyInfos
                    };

                });
                return cacheEntry;
            }
            finally
            {
                releaser.Dispose();
            }

        }

        public void SetOptions(KeyVaultStoreOptions options)
        {
            _options = options;
        }

        public async Task CreateKeysAsync(DateTime utcStartTime,int count, int expirationDays, int overlappingDays)
        {
            if(count > 50)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if(expirationDays <= overlappingDays)
            {
                throw new ArgumentOutOfRangeException(nameof(overlappingDays));
            }
            var keyClient = _azureKeyVaultClients.CreateKeyClient(_options.KeyVaultUrl);
            var utcNow = utcStartTime;
            for(int i = 0; i < count; i++)
            {
                var createEcKeyOptions = new CreateEcKeyOptions(_options.KeyIdentifier)
                {
                    CurveName = KeyCurveName.P256,
                    NotBefore = utcNow,
                    ExpiresOn = utcNow.AddDays(expirationDays),
                };
                createEcKeyOptions.KeyOperations.Add(new KeyOperation("sign"));
                createEcKeyOptions.KeyOperations.Add(new KeyOperation("verify"));
                var response = await keyClient.CreateEcKeyAsync(createEcKeyOptions) ;
                utcNow = utcNow.AddDays(expirationDays - overlappingDays);
            }
        }
    }
}
