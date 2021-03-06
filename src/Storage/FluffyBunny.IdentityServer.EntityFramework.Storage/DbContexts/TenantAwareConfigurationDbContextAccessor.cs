﻿using System;
using Duende.IdentityServer.EntityFramework.Options;
using FluffyBunny.EntityFramework.Context;

namespace Microsoft.EntityFrameworkCore
{
    public class TenantAwareConfigurationDbContextAccessor : ITenantAwareConfigurationDbContextAccessor
    {

        private IServiceProvider _serviceProvider;
        private IDbContextOptionsProvider _dbContextOptionsProvider;
        private ConfigurationStoreOptions _storeOptions;
        private OperationalStoreOptions _operationalStoreOptions;

        public TenantAwareConfigurationDbContextAccessor(
            IServiceProvider serviceProvider,
            ConfigurationStoreOptions storeOptions,
            OperationalStoreOptions operationalStoreOptions,
            IDbContextOptionsProvider dbContextOptionsProvider)
        {
            _serviceProvider = serviceProvider;
            _storeOptions = storeOptions;
            _operationalStoreOptions = operationalStoreOptions;
            _dbContextOptionsProvider = dbContextOptionsProvider;
        }
        public ITenantAwareConfigurationDbContext GetTenantAwareConfigurationDbContext(string tenantId)
        {
            var dbContext = new TenantAwareConfigurationDbContext(tenantId, _storeOptions, _operationalStoreOptions, _dbContextOptionsProvider);
            return dbContext;
        }

      
    }
}