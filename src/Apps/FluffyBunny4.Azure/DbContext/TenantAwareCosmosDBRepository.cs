using FluffyBunny4.Azure.Configuration.CosmosDB;
using FluffyBunny4.DotNetCore.Services;
using FluffyBunny4.Services;
using FluffyBunny4.Stores;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using FluffyBunny.EntityFramework.Context;
using FluffyBunny4.Models;

namespace FluffyBunny4.Azure.DbContext
{
    public enum TenantContainerType
    {
        Operational,
        Clients,
        ApiScopes,
        ApiResources
    }
    
    public abstract class TenantAwareCosmosDBRepository<T> : CosmosDBRepository<T>
       where T : class
    {
        protected class CollectionConfiguration 
        {
            public Collection Collection { get; set; }
            public string DatabaseName { get; set; }
        
        }
        protected IScopedContext<TenantContext> _scopedTenantContext;
        protected ITenantStore _tenantStore;
        private IHostStorage _hostStorage;

        public TenantAwareCosmosDBRepository(
            IScopedContext<TenantContext> scopedTenantContext,
            ITenantStore tenantStore,
            IHostStorage hostStorage,
            IOptions<CosmosDbConfiguration> settings,
            ConnectionPolicy connectionPolicy = null,
            ILogger logger = null) : base(settings, connectionPolicy, logger)
        {
            _scopedTenantContext = scopedTenantContext;
            _tenantStore = tenantStore;
            _hostStorage = hostStorage;
        }
        public override async Task<Container> GetContainerAsync()
        {
            /*
             {
                "TenantId": "herb",
                "Name": "herb's Tenant",
                "Enabled": true,
                "Properties": {
                    "cosmos:operational:databaseName": "herb-database",
                    "cosmos:operational": "herb-operational",
                    "cosmos:clients": "herb-clients"
                }
              }
             */
            object objContainer = null;
            string key = GetKey();
            if (!_hostStorage.TryGetValue(key, out objContainer))
            {

                var collectionConfiguration = await GetCollectionConfigurationAsync();
                await EnsureDatabaseCreatedAsync(collectionConfiguration.DatabaseName);
                objContainer = CosmosClient.GetContainer(
                    DatabaseV3.Id, collectionConfiguration.Collection.CollectionName);
                _hostStorage.AddOrUpdate(key, objContainer);
                await CreateContainerIfNotExistsAsync(collectionConfiguration.Collection);
            }
            return objContainer as Container;
        }

        protected abstract string GetKey();
        protected abstract Task<CollectionConfiguration> GetCollectionConfigurationAsync();
    }
} 