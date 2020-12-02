using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace FluffyBunny4.Azure.Services
{
    public interface ICosmosDbContext<T> where T : class
    {
        Task<TDoc> GetOneAsync<TDoc>(QueryDefinition query, PartitionKey partitionKey);
    }
}
