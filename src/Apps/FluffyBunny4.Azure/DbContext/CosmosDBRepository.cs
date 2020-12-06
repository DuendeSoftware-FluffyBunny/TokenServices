using System;
using System.Collections.Generic;
using System.IO;
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
using Newtonsoft.Json;

namespace FluffyBunny4.Azure.DbContext
{

    public abstract class CosmosDBRepository<T> :
        CosmosDbContextBase<T>,
        ISimpleItemDbContext<T>,
        ICosmosDbContext<T>
        where T : class
    {
        private Uri _documentCollectionUri;

        public Uri DocumentCollectionUri => _documentCollectionUri;

        protected abstract Task<Collection> GetCollectionAsync();
        protected abstract string GetPartitionKey(ref T item);
        protected abstract string GetPartitionKeyPath();
        public abstract Task<Container> GetContainerAsync();

        public CosmosDBRepository(
            IOptions<CosmosDbConfiguration> settings,
            ConnectionPolicy connectionPolicy = null,
            ILogger logger = null) :
            base(settings, connectionPolicy, logger)
        {
            Guard.ForNullOrDefault(settings.Value, nameof(settings));
        }


        public async Task<TDoc> GetOneAsync<TDoc>(QueryDefinition query, Microsoft.Azure.Cosmos.PartitionKey partitionKey)
        {
            var container = await GetContainerAsync();
            List<TDoc> results = new List<TDoc>();
            FeedIterator<TDoc> resultSetIterator = container.GetItemQueryIterator<TDoc>(query,
                requestOptions: new QueryRequestOptions() { PartitionKey = partitionKey });
            while (resultSetIterator.HasMoreResults)
            {
                Microsoft.Azure.Cosmos.FeedResponse<TDoc> response = await resultSetIterator.ReadNextAsync();
                results.AddRange(response);
                if (response.Diagnostics != null)
                {
                    _logger.LogError($"QueryWithSqlParameters Diagnostics: {response.Diagnostics.ToString()}");
                }
                break;
            }
            var item = results.FirstOrDefault();
            return item;
        }
        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                var container = await GetContainerAsync();
                var partitionKey = new Microsoft.Azure.Cosmos.PartitionKey(id);

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
                            //                            Console.WriteLine($"Read item from stream failed. Status code: {responseMessage.StatusCode} Message: {responseMessage.ErrorMessage}");
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
            string partitionKey = GetPartitionKey(ref item);
            var response = await container.UpsertItemAsync(item, new Microsoft.Azure.Cosmos.PartitionKey(partitionKey));
            return response;
        }

       

        public async Task<Document> ReplaceItemAsync(string id, T item)
        {
            // TODO swap this for V3
//            return await DocumentClient.ReplaceDocumentAsync(_documentCollectionUri, item);
            throw new NotImplementedException();
        }

        public async Task<ItemResponse<T>> DeleteItemAsync(string id)
        {
            var container = await GetContainerAsync();
            var pk = new Microsoft.Azure.Cosmos.PartitionKey(id);
            ItemResponse<T> response = await container.DeleteItemAsync<T>(partitionKey: pk, id: id);
            return response;
        }

        protected async Task CreateContainerIfNotExistsAsync(Collection collection)
        {
            // Create new container
            var containerProperties = new ContainerProperties
            {
                Id = collection.CollectionName,
                PartitionKeyPath = GetPartitionKeyPath(),        // "/id"
                DefaultTimeToLive = -1,
                IndexingPolicy = new Microsoft.Azure.Cosmos.IndexingPolicy()
                {
                    IndexingMode = Microsoft.Azure.Cosmos.IndexingMode.Lazy,
                    Automatic = true

                }
            };


            var containerResponse = await DatabaseV3.CreateContainerIfNotExistsAsync(containerProperties);
            if (!containerResponse.StatusCode.IsSuccess())
            {
                var message = $"Cant Create CosmosContainer.  Database:{DatabaseV3.Id}, Container:{collection.CollectionName}";
                _logger.LogError(message);
                throw new Exception(message);
            }

            //await DatabaseV3.CreateContainerIfNotExistsAsync(containerProperties);

        }

    }
}
