using FluffyBunny4.Azure.Extensions;
using FluffyBunny4.Azure.Models;
using FluffyBunny4.Azure.Services;
using FluffyBunny4.DotNetCore;
using FluffyBunny4.DotNetCore.Extensions;
using FluffyBunny4.Stores;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

namespace FluffyBunny4.Azure.Stores.CosmosDB
{
    public class PersistedGrantStore : IPersistedGrantStoreEx
    {
        private ISimpleItemDbContext<PersistedGrantCosmosDocument> _simpleItemDbContext;
        private ILogger _logger;

        public PersistedGrantStore(
           ISimpleItemDbContext<PersistedGrantCosmosDocument> simpleItemDbContext,
           ILogger<PersistedGrantStore> logger)
        {
            _simpleItemDbContext = simpleItemDbContext;
            _logger = logger;
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            QueryDefinition query = new QueryDefinition("SELECT * FROM operational f WHERE f.subjectId = @subjectId")
                .WithParameter("@subjectId", subjectId);

            List<PersistedGrantCosmosDocument> results = new List<PersistedGrantCosmosDocument>();
            var container = await _simpleItemDbContext.GetContainerAsync();
            FeedIterator<PersistedGrantCosmosDocument> resultSetIterator = container.GetItemQueryIterator<PersistedGrantCosmosDocument>(query,
                requestOptions: new QueryRequestOptions() { });
            while (resultSetIterator.HasMoreResults)
            {
                Microsoft.Azure.Cosmos.FeedResponse<PersistedGrantCosmosDocument> response = await resultSetIterator.ReadNextAsync();
                results.AddRange(response);
                if (response.Diagnostics != null)
                {
                    Console.WriteLine($"\nQueryWithSqlParameters Diagnostics: {response.Diagnostics.ToString()}");
                }
            }

            var persistedGrants = from item in results
                                  select item.ToPersistedGrant();

            return persistedGrants;
        }

        public async Task<PersistedGrant> GetAsync(string key)
        {
            Guard.ArgumentNotNullOrEmpty(nameof(key), key);
            var item = await _simpleItemDbContext.GetItemAsync(key);
            return item.ToPersistedGrant();
        }


        private static Task<ItemResponse<PersistedGrantCosmosDocument>> ExecuteDeleteAsync(Container container, PersistedGrantCosmosDocument item)
        {
            return container.DeleteItemAsync<PersistedGrantCosmosDocument>(item.Id, new PartitionKey(item.Id));
        }
        public async Task RemoveAllAsync(string subjectId)
        {
            await RemoveAllAsync(subjectId, before: null);
        }
        public async Task RemoveAllAsync(string subjectId, string clientId)
        {
            await RemoveAllAsync(subjectId, clientId, before: null);
        }
        public async Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            await RemoveAllAsync(subjectId, clientId, type, before: null);
        }

        public async Task RemoveAsync(string id)
        {
            QueryDefinition query = new QueryDefinition("SELECT * FROM operational f WHERE f.id = @id")
               .WithParameter("@id", id);
            await RemoveAllAsync(query);
        }

        public async Task StoreAsync(PersistedGrant grant)
        {
            Guard.ArgumentNotNull(nameof(grant), grant);
            var document = grant.ToPersistedGrantCosmosDocument();
            var response = await _simpleItemDbContext.UpsertItemAsync(document);
            if (!response.StatusCode.IsSuccess())
            {
                _logger.LogError($"StatusCode={response.StatusCode}");
            }
        }
        public async Task RemoveAllAsync(string subjectId, DateTime? before)
        {
            var nowUTC = DateTime.UtcNow;
            if (before != null)
            {
                nowUTC = (DateTime)before;
            }

            QueryDefinition query = new QueryDefinition("SELECT * FROM operational f WHERE f.subjectId = @subjectId  AND f.creationTime < @nowUTC")
               .WithParameter("@subjectId", subjectId)
               .WithParameter("@nowUTC", nowUTC.ToString("o"));
            await RemoveAllAsync(query);
        }
        public async Task RemoveAllAsync(string subjectId, string clientId, DateTime? before)
        {
            var nowUTC = DateTime.UtcNow;
            if (before != null)
            {
                nowUTC = (DateTime)before;
            }

            QueryDefinition query = new QueryDefinition("SELECT * FROM operational f WHERE f.subjectId = @subjectId AND f.clientId = @clientId AND f.creationTime < @nowUTC")
               .WithParameter("@subjectId", subjectId)
               .WithParameter("@clientId", clientId)
               .WithParameter("@nowUTC", nowUTC.ToString("o"));
            await RemoveAllAsync(query);
        }

        public async Task RemoveAllAsync(string subjectId, string clientId, string type, DateTime? before)
        {
            var nowUTC = DateTime.UtcNow;
            if (before != null)
            {
                nowUTC = (DateTime)before;
            }

            QueryDefinition query = new QueryDefinition("SELECT * FROM operational f WHERE f.subjectId = @subjectId AND f.clientId = @clientId AND f.type = @type AND f.creationTime < @nowUTC")
               .WithParameter("@type", type)
               .WithParameter("@subjectId", subjectId)
               .WithParameter("@clientId", clientId)
               .WithParameter("@nowUTC", nowUTC.ToString("o"));
            await RemoveAllAsync(query);
        }

        private async Task RemoveAllAsync(QueryDefinition query)
        {
            try
            {
                List<Task<ItemResponse<PersistedGrantCosmosDocument>>> deleteTasks = new List<Task<ItemResponse<PersistedGrantCosmosDocument>>>();
                bool bContinue = true;
                while (bContinue)
                {
                    bContinue = false;
                    List<PersistedGrantCosmosDocument> results = new List<PersistedGrantCosmosDocument>();
                    var container = await _simpleItemDbContext.GetContainerAsync();
                    FeedIterator<PersistedGrantCosmosDocument> resultSetIterator = container.GetItemQueryIterator<PersistedGrantCosmosDocument>(query,
                        requestOptions: new QueryRequestOptions() { });
                    if (resultSetIterator.HasMoreResults)
                    {
                        Microsoft.Azure.Cosmos.FeedResponse<PersistedGrantCosmosDocument> response = await resultSetIterator.ReadNextAsync();
                        results.AddRange(response);

                        if (response.Diagnostics != null)
                        {
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                _logger.LogDebug($"\nQueryWithSqlParameters Diagnostics: {response.Diagnostics}");
                            }
                        }
                        if (results.Any())
                        {
                            foreach (var item in results)
                            {
                                deleteTasks.Add(ExecuteDeleteAsync(container, item));
                            }
                            await Task.WhenAll(deleteTasks);
                            bContinue = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            // TODO.  Cover all the bases.  It currently looks like the default service is only asking for subject id
            return await GetAllAsync(filter.SubjectId);

        }

        public async Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.SubjectId))
            {
                if (!string.IsNullOrWhiteSpace(filter.ClientId))
                {
                    if (!string.IsNullOrWhiteSpace(filter.Type))
                    {
                        await RemoveAllAsync(filter.SubjectId, filter.ClientId, filter.Type);
                    }
                    else
                    {
                        await RemoveAllAsync(filter.SubjectId, filter.ClientId);
                    }
                }
                else
                {
                    await RemoveAllAsync(filter.SubjectId);
                }
            }
        }

        public Task CopyAsync(string sourceKey, string destinationKey)
        {
            throw new NotImplementedException();
        }
    }
}
