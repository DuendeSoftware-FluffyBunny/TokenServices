using FluffyBunny4.Configuration;
using FluffyBunny4.DotNetCore;
using FluffyBunny4.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FluffyBunny4.DotNetCore.TimedLock;

namespace FluffyBunny4.Services
{
    public class TenantResolver : IKeyVaultTenantResolver
    {
        private KeyVaultStoreOptions _options;
        private IServiceProvider _serviceProvider;
        private ITenantStore _tenantStore;
        private ILogger<TenantResolver> _logger;
     
        public TenantResolver(
            IOptions<KeyVaultStoreOptions> options,
            IServiceProvider serviceProvider,
            ITenantStore tenantStore,
            ILogger<TenantResolver> logger)
        {
            _options = options.Value;
            _serviceProvider = serviceProvider;
            _tenantStore = tenantStore;
            _logger = logger;
        }
    

        public async Task<IJwksDiscovery> GetJwksDisoveryAsync(string tenantId)
        {
            if (!await IsTenantValidAsync(tenantId))
            {
                throw new Exception($"Invalid Tenant({tenantId}");
            }
            var keyVaultECDsaKeyStore = 
                _serviceProvider.GetRequiredService<KeyVaultECDsaKeyStore>();
            var configuration = keyVaultECDsaKeyStore as IKeyVaultECDsaKeyStoreConfiguration;
            KeyVaultStoreOptions options = (KeyVaultStoreOptions)_options.Clone();

            options.KeyIdentifier = string.Format(_options.KeyIdentifier, tenantId);
            configuration.SetOptions(options);
            return keyVaultECDsaKeyStore as IJwksDiscovery;
        }

        public async Task<IKeyVaultECDsaKeyStore> GetKeyVaultECDsaKeyStoreAsync(string tenantId)
        {
            if (!await IsTenantValidAsync(tenantId))
            {
                throw new Exception($"Invalid Tenant({tenantId}");
            }
            var keyVaultECDsaKeyStore =
                _serviceProvider.GetRequiredService<KeyVaultECDsaKeyStore>();
            var configuration = keyVaultECDsaKeyStore as IKeyVaultECDsaKeyStoreConfiguration;
            KeyVaultStoreOptions options = (KeyVaultStoreOptions)_options.Clone();

            options.KeyIdentifier = string.Format(_options.KeyIdentifier,tenantId);
            configuration.SetOptions(options);
            var ikeyVaultECDsaKeyStore = keyVaultECDsaKeyStore as IKeyVaultECDsaKeyStore;
            return ikeyVaultECDsaKeyStore;

        }

        public async Task<ISignatureProvider> GetSignatureProviderAsync(string tenantId)
        {
            if (!await IsTenantValidAsync(tenantId))
            {
                throw new Exception($"Invalid Tenant({tenantId}");
            }
            var azureKeyVaultECDsaSignatureProvider = _serviceProvider.GetRequiredService<AzureKeyVaultECDsaSignatureProvider>();
            var configAzureKeyVaultECDsaSignatureProvider = azureKeyVaultECDsaSignatureProvider as IAzureKeyVaultECDsaSignatureProviderConfiguration;
            var ikeyVaultECDsaKeyStore = await GetKeyVaultECDsaKeyStoreAsync(tenantId);
            configAzureKeyVaultECDsaSignatureProvider.SetKeyVaultECDsaKeyStore(ikeyVaultECDsaKeyStore);
            return azureKeyVaultECDsaSignatureProvider as ISignatureProvider;
        }

        public async Task<bool> IsTenantValidAsync(string tenantId)
        {
            var tenant = await _tenantStore.FindTenantByIdAsync(tenantId);
            return (tenant != null && tenant.Enabled);
        }
    }
}
