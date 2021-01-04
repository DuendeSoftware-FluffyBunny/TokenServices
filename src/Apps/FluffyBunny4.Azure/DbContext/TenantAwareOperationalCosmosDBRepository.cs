using FluffyBunny4.Azure.Configuration.CosmosDB;
using FluffyBunny4.Azure.Models;
using FluffyBunny4.DotNetCore.Services;
using FluffyBunny4.Services;
using FluffyBunny4.Stores;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using FluffyBunny.EntityFramework.Context;
using FluffyBunny4.Models;

namespace FluffyBunny4.Azure.DbContext
{
    public class TenantAwareOperationalCosmosDBRepository : TenantAwareCosmosDBRepository<PersistedGrantCosmosDocument>
    {
        private CollectionConfiguration _collectionConfiguration;

        public string Key { get; }
        public TenantAwareOperationalCosmosDBRepository(
            IScopedContext<TenantRequestContext> scopedTenantRequestContext,
            ITenantStore tenantStore,
            IHostStorage hostStorage,
            IOptions<CosmosDbConfiguration> settings,
            ConnectionPolicy connectionPolicy = null,
            ILogger logger = null) :
            base(scopedTenantRequestContext, tenantStore, hostStorage, settings, connectionPolicy, logger)
        {
            Key = $"{scopedTenantRequestContext.Context.TenantName}:{this.GetType().FullName}:{TenantContainerType.Operational}";
        }
        protected override string GetPartitionKeyPath()
        {
            return "/id";
        }
        protected override string GetPartitionKey(ref PersistedGrantCosmosDocument item)
        {
            return item.Id;
        }
        protected async override Task<Collection> GetCollectionAsync()
        {
            var cc = await GetCollectionConfigurationAsync();
            return cc.Collection;
        }
        protected async override Task<CollectionConfiguration> GetCollectionConfigurationAsync()
        {
            if(_collectionConfiguration == null)
            {
                var tenant = await _tenantStore.FindTenantByIdAsync(_scopedTenantRequestContext.Context.TenantName);
                _collectionConfiguration = new CollectionConfiguration
                {
                    Collection = new Collection
                    {
                        CollectionName = tenant.Properties[Constants.Cosmos.OperationalContainerName],
                        ReserveUnits = 400
                    },
                    DatabaseName = tenant.Properties[Constants.Cosmos.OperationalDatabaseName]
                };
            }
            return _collectionConfiguration;
        }
        protected override string GetKey()
        {
            return Key;
        }
    }
} 