using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluffyBunny4.Azure.Configuration.CosmosDB;
using FluffyBunny4.DotNetCore;
using FluffyBunny4.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

namespace FluffyBunny4.Azure.Abstracts
{
    /// <summary>
    ///     Base class for Context that uses CosmosDb.
    /// </summary>
    public abstract class CosmosDbContextBase<T> : IDisposable
        where T : class
    {
        /// <summary>
        ///     CosmosDb Configuration Data
        /// </summary>
        protected readonly CosmosDbConfiguration Configuration;

        /// <summary>
        ///     Logger
        /// </summary>
        protected readonly ILogger _logger;
        public CosmosClient CosmosClient { get; }
        /// <summary>
        ///     Protected Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="connectionPolicy"></param>
        /// <param name="logger"></param>
        protected CosmosDbContextBase(
            IOptions<CosmosDbConfiguration> settings,
            ConnectionPolicy connectionPolicy = null,
            ILogger logger = null)
        {
            Guard.ForNullOrDefault(settings.Value, nameof(settings));
            Guard.ForNullOrDefault(settings.Value.EndPointUrl, nameof(settings.Value.EndPointUrl));
            Guard.ForNullOrDefault(settings.Value.PrimaryKey, nameof(settings.Value.PrimaryKey));
            _logger = logger;
            Configuration = settings.Value;

            var serviceEndPoint = new Uri(settings.Value.EndPointUrl);

            if (Configuration.DangerousAcceptAnyServerCertificateValidator)
            {
                CosmosClientOptions cosmosClientOptions = new CosmosClientOptions()
                {
                    HttpClientFactory = () =>
                    {
                        HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                        {
                            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        };

                        return new HttpClient(httpMessageHandler);
                    },
                    ConnectionMode = Microsoft.Azure.Cosmos.ConnectionMode.Gateway
                };
                CosmosClient = new CosmosClient(serviceEndPoint.AbsoluteUri, settings.Value.PrimaryKey, cosmosClientOptions);
            }
            else
            {

                CosmosClientBuilder configurationBuilder = new CosmosClientBuilder(serviceEndPoint.AbsoluteUri, settings.Value.PrimaryKey);
                CosmosClient = configurationBuilder.Build();
            }

   //         DocumentClient = new DocumentClient(serviceEndPoint, settings.Value.PrimaryKey,connectionPolicy ?? ConnectionPolicy.Default);

//            EnsureDatabaseCreated(Configuration.DatabaseName).Wait();
        }



        /// <summary>
        ///     CosmosDb Document Client.
        /// </summary>
 //       public DocumentClient DocumentClient { get; }

        /// <summary>
        ///     Instance of CosmosDb Database.
        /// </summary>
        protected Microsoft.Azure.Cosmos.Database DatabaseV3 { get; private set; }
        /// <summary>
        ///     URL for CosmosDb Instance.
        /// </summary>
        protected Uri DatabaseUri { get; private set; }

        /// <summary>
        ///     Dispose of object resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

          

        protected async Task EnsureDatabaseCreatedAsync(string databaseName)
        {
            // Create new database
            DatabaseV3 = await CosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
            DatabaseUri = UriFactory.CreateDatabaseUri(databaseName);
            _logger?.LogDebug($"Database URI: {DatabaseUri}");
        }

        /// <summary>
        ///     Dispose of object resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO : Dispose of any resources
            }
        }

        /// <summary>
        ///     Deconstructor
        /// </summary>
        ~CosmosDbContextBase()
        {
            Dispose(false);
        }
    }
}
