using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluffyBunny4.Azure.Abstracts;
using FluffyBunny4.Azure.Configuration.CosmosDB;
using FluffyBunny4.Azure.Extensions;
using FluffyBunny4.Azure.Services;
using FluffyBunny4.DotNetCore;
using FluffyBunny4.DotNetCore.Extensions;
using FluffyBunny4.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluffyBunny4.Azure.DbContext
{
    public abstract class DocumentDBRepository<T> :
        CosmosDbContextBase<T>,
        ISimpleItemDbContext<T>
        where T : class
    {
        public Collection Collection { get; }

        private ILogger _logger;

        protected abstract string GetPartitionKey(ref T item);
        protected abstract Collection GetCollectionConfiguration();
        public abstract Task<Container> GetContainerAsync();

        public DocumentDBRepository(
            IOptions<CosmosDbConfiguration> settings,
            ConnectionPolicy connectionPolicy = null,
            ILogger logger = null) :
            base(settings, connectionPolicy, logger)
        {
            Collection = GetCollectionConfiguration();
            _logger = logger;
            Guard.ForNullOrDefault(settings.Value, nameof(settings));
        }
        public Uri DocumentCollectionUri { get; set; }

        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                var partitionKey = new Microsoft.Azure.Cosmos.PartitionKey(id);
                var container = await GetContainerAsync();
                ItemResponse<T> response = await container.ReadItemAsync<T>(
                      partitionKey: partitionKey,
                      id: id);
                if (response.StatusCode.IsSuccess())
                {
                    T item = response;
                    // Read the same item but as a stream.
                    using (ResponseMessage responseMessage = await container.ReadItemStreamAsync(
                        partitionKey: partitionKey,
                        id: id))
                    {
                        // Item stream operations do not throw exceptions for better performance
                        if (responseMessage.IsSuccessStatusCode)
                        {
                            T streamResponse = responseMessage.Content.FromStream<T>();
                            return streamResponse;
                        }
                        else
                        {
                            _logger.LogError($"Read item from stream failed. Status code: {responseMessage.StatusCode} Message: {responseMessage.ErrorMessage}");
                        }
                    }

                }

                return null;
            }

            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ItemResponse<T>> UpsertItemAsync(T item)
        {
            var container = await GetContainerAsync();
            var partitionKey = GetPartitionKey(ref item);
            var response = await container.UpsertItemAsync(item, new Microsoft.Azure.Cosmos.PartitionKey(partitionKey));
            return response;
        }

        public async Task<Document> ReplaceItemAsync(string id, T item)
        {
            return await DocumentClient.ReplaceDocumentAsync(DocumentCollectionUri, item);
        }

        public async Task<ItemResponse<T>> DeleteItemAsync(string id)
        {
            var pk = new Microsoft.Azure.Cosmos.PartitionKey(id);
            var container = await GetContainerAsync();

            ItemResponse<T> response = await container.DeleteItemAsync<T>(partitionKey: pk, id: id);

            return response;
        }

        protected async Task CreateContainerIfNotExistsAsync(Collection collection, Uri documentCollectionUri)
        {
            // Create new container
            var containerProperties = new ContainerProperties
            {
                Id = collection.CollectionName,
                PartitionKeyPath = "/id",
                DefaultTimeToLive = -1
            };

            var containerResponse = await DatabaseV3.CreateContainerIfNotExistsAsync(containerProperties);
            if (!containerResponse.StatusCode.IsSuccess())
            {
                var message = $"Cant Create CosmosContainer.  Database:{DatabaseV3.Id}, Container:{collection.CollectionName}";
                _logger.LogError(message);
                throw new Exception(message);
            }
        }
    }
}
