using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Duende.IdentityServer.EntityFramework;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Services
{
    class TenantAwareTokenCleanupService
    {
        private OperationalStoreOptions _options;
        private IAdminServices _adminServices;
        private ITenantAwareConfigurationDbContextAccessor _tenantAwareConfigurationDbContextAccessor;
        private ILogger<TenantAwareTokenCleanupService> _logger;
        private IOperationalStoreNotification _operationalStoreNotification;

        public TenantAwareTokenCleanupService(
            OperationalStoreOptions options,
            IAdminServices adminServices,
            ITenantAwareConfigurationDbContextAccessor tenantAwareConfigurationDbContextAccessor,
            ILogger<TenantAwareTokenCleanupService> logger,
            IOperationalStoreNotification operationalStoreNotification = null)
        {
            _options = options;
            _adminServices = adminServices;
            _tenantAwareConfigurationDbContextAccessor = tenantAwareConfigurationDbContextAccessor;
            _logger = logger;
            _operationalStoreNotification = operationalStoreNotification;
        }

        ITenantAwareConfigurationDbContext GetTenantContext(string name)
        {
            name = name.ToLower();
            return _tenantAwareConfigurationDbContextAccessor.GetTenantAwareConfigurationDbContext(name);
        }

        public async Task RemoveExpiredDeviceCodesAsync()
        {
            try
            {
                var tenants = await _adminServices.GetAllTenantsAsync();
                foreach (var tenant in tenants)
                {
                    var context = GetTenantContext(tenant.Name);

                    var found = Int32.MaxValue;

                    while (found >= _options.TokenCleanupBatchSize)
                    {
                        var expiredCodes = await context.DeviceFlowCodes
                            .Where(x => x.Expiration < DateTime.UtcNow)
                            .OrderBy(x => x.DeviceCode)
                            .Take(_options.TokenCleanupBatchSize)
                            .ToArrayAsync();

                        found = expiredCodes.Length;
                        _logger.LogInformation("Removing {deviceCodeCount} device flow codes", found);

                        if (found > 0)
                        {
                            context.DeviceFlowCodes.RemoveRange(expiredCodes);
                            await context.SaveChangesAsync();

                            if (_operationalStoreNotification != null)
                            {
                                await _operationalStoreNotification.DeviceCodesRemovedAsync(expiredCodes);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception removing expired device codes: {exception}", ex.Message);
            }
        }

        public async Task RemoveExpiredGrantsAsync()
        {
            try
            {
                var tenants = await _adminServices.GetAllTenantsAsync();
                foreach (var tenant in tenants)
                {
                    var context = GetTenantContext(tenant.Name);

                    var found = Int32.MaxValue;

                    while (found >= _options.TokenCleanupBatchSize)
                    {
                        var expiredGrants = await context.PersistedGrants
                            .Where(x => x.Expiration < DateTime.UtcNow)
                            .OrderBy(x => x.Key)
                            .Take(_options.TokenCleanupBatchSize)
                            .ToArrayAsync();

                        found = expiredGrants.Length;
                        _logger.LogInformation("Removing {grantCount} grants", found);

                        if (found > 0)
                        {
                            context.PersistedGrants.RemoveRange(expiredGrants);
                            await context.SaveChangesAsync();

                            if (_operationalStoreNotification != null)
                            {
                                await _operationalStoreNotification.PersistedGrantsRemovedAsync(expiredGrants);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception removing expired grants: {exception}", ex.Message);
            }
        }
    }
}
