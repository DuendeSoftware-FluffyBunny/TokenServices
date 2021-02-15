using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Models;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using FluffyBunny4.DotNetCore;
using FluffyBunny4.DotNetCore.Services;
using FluffyBunny4.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Stores
{

    public class SelfManagedValidationKeysStore :
        IValidationKeysStore,
        IKeyMaterialService
    {
        private class KeyInfoContainer
        {
            public SecurityKeyInfo SecurityKeyInfo { get; set; }
            public SigningCredentials SigningCredentials { get; set; }
            public DateTime NotBefore { get; set; }
            public DateTime Expiration { get; set; }
        }

        private IScopedContext<TenantRequestContext> _scopedTenantRequestContext;
        private SelfManagedCertificatesOptions _selfManagedCertificatesOptions;
        private IAdminServices _adminServices;
        private ILogger _logger;
        private IMemoryCache _memoryCache;
        private ISerializer _serializer;
        private TimedLock _lockGetKeyInfoContainersAsync;
        private TimedLock _lockGetSigningCredentialsAsync;
        private TimedLock _lockGetAllSigningCredentialsAsync;
        public SelfManagedValidationKeysStore(
            IScopedContext<TenantRequestContext> scopedTenantRequestContext,
            IOptions<SelfManagedCertificatesOptions> selfManagedCertificatesOptions,
            IAdminServices adminServices,
            IMemoryCache memoryCache,
            ISerializer serializer,
            ILogger<SelfManagedValidationKeysStore> logger)
        {
            _scopedTenantRequestContext = scopedTenantRequestContext;
            _selfManagedCertificatesOptions = selfManagedCertificatesOptions.Value;
            _adminServices = adminServices;
            _memoryCache = memoryCache;
            _serializer = serializer;
            _lockGetKeyInfoContainersAsync = new TimedLock();
            _lockGetSigningCredentialsAsync = new TimedLock();
            _lockGetAllSigningCredentialsAsync = new TimedLock();
            _logger = logger;
        }

        private async Task<IEnumerable<KeyInfoContainer>> GetKeyInfoContainersAsync()
        {
            var utcNow = DateTime.UtcNow;

            var tenantName = _scopedTenantRequestContext.Context.TenantName;
            var cacheKey = $"{tenantName}-GetKeyInfoContainersAsync";
            TimedLock.LockReleaser releaser = await _lockGetKeyInfoContainersAsync.Lock(new TimeSpan(0, 0, 30));
            try
            {
                var cacheEntry = await _memoryCache.GetOrCreate(cacheKey, async entry =>
                {
                    /*
                     * There is a 60 month overlap, so we show the previous key infos for 3 months to give
                     * downstream validators time to catchup and pull the new public certs.
                     */
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

                    var certificates = await _adminServices.GetAllCertificatesAsync(
                        tenantName, "ECDsa",
                        utcNow.AddMonths(-9), // 3 month showing for keys we don't use anymore
                        utcNow.AddMonths(12) // here we pick up keys we are going to use in the future.
                    );

                    /*
                     * typically you should see 3 key infos in the JWKS discovery.
                     * 1. The one we stopped using
                     * 2. the one we are using
                     * 3. the one we are going to use in the future.
                     */

                    List<KeyInfoContainer> keys = new List<KeyInfoContainer>();
                    foreach (var cert in certificates)
                    {
                        byte[] ecdsaCertPfxBytes = Convert.FromBase64String(cert.PFXBase64);
                        var ecdsaCertificate = new X509Certificate2(ecdsaCertPfxBytes, _selfManagedCertificatesOptions.Password);
                        ECDsaSecurityKey ecdsaCertificatePublicKey =
                            new ECDsaSecurityKey(ecdsaCertificate.GetECDsaPrivateKey());
                        ecdsaCertificatePublicKey.KeyId = cert.PFXBase64.Sha256();
                        var credential = new SigningCredentials(ecdsaCertificatePublicKey,
                            GetECDsaSigningAlgorithmValue(IdentityServerConstants.ECDsaSigningAlgorithm.ES256));
                      
                        var keyInfo = new SecurityKeyInfo
                        {
                            Key = credential.Key,
                            SigningAlgorithm = credential.Algorithm
                            
                        };
                        keys.Add(new KeyInfoContainer
                        {
                            Expiration = cert.Expiration,
                            NotBefore = cert.NotBefore,
                            SecurityKeyInfo = keyInfo,
                            SigningCredentials = credential
                        });
                    }

                    return keys;

                });
                return cacheEntry;
            }
            finally
            {
                releaser.Dispose();
            }
        }

        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            var keyInfoContainers = await GetKeyInfoContainersAsync();

            var query = from item in keyInfoContainers
                let c = item.SecurityKeyInfo
                select c;
            return query;
        }


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

        public async Task<SigningCredentials> GetSigningCredentialsAsync(IEnumerable<string> allowedAlgorithms = null)
        {
            var utcNow = DateTime.UtcNow;

            var tenantName = _scopedTenantRequestContext.Context.TenantName;
            var cacheKey = $"{tenantName}-GetSigningCredentialsAsync";
            TimedLock.LockReleaser releaser = await _lockGetSigningCredentialsAsync.Lock(new TimeSpan(0, 0, 30));
            try
            {
                var cacheEntry = await _memoryCache.GetOrCreate(cacheKey, async entry =>
                {
                    var keyInfoContainers = await GetKeyInfoContainersAsync();
                    var query = from item in keyInfoContainers
                        where 
                            item.NotBefore > utcNow.AddMonths(-6).AddDays(-15) 
                            && item.NotBefore < utcNow
                        orderby item.NotBefore descending
                        select item;
                    return query.FirstOrDefault().SigningCredentials;

                });
                return cacheEntry;
            }
            finally
            {
                releaser.Dispose();
            }

        }

        public async Task<IEnumerable<SigningCredentials>> GetAllSigningCredentialsAsync()
        {
            var utcNow = DateTime.UtcNow;

            var tenantName = _scopedTenantRequestContext.Context.TenantName;
            var cacheKey = $"{tenantName}-GetAllSigningCredentialsAsync";
            TimedLock.LockReleaser releaser = await _lockGetAllSigningCredentialsAsync.Lock(new TimeSpan(0, 0, 30));
            try
            {
                var cacheEntry = await _memoryCache.GetOrCreate(cacheKey, async entry =>
                {
                    var keyInfoContainers = await GetKeyInfoContainersAsync();
                    var query = from item in keyInfoContainers
                        let c = item.SigningCredentials
                        select c;
                    return query;

                });
                return cacheEntry;
            }
            finally
            {
                releaser.Dispose();
            }
           

        }
    }

}
