using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Stores.Serialization;
using Microsoft.Extensions.Logging;

namespace FluffyBunny4.Stores
{
    // Only need this because the cosmos ids have chars that are excluded.
    public class MyDefaultReferenceTokenStore : DefaultReferenceTokenStore
    {
        public MyDefaultReferenceTokenStore(
            IPersistedGrantStore store, 
            IPersistentGrantSerializer serializer, 
            IHandleGenerationService handleGenerationService, 
            ILogger<DefaultReferenceTokenStore> logger) : 
            base(store, serializer, handleGenerationService, logger)
        {
        }
        protected override string GetHashedKey(string value)
        {
            // COSMOS
            // >> The following characters are restricted and cannot be used in the Id property: '/', '\\', '?', '#'
            var ori = base.GetHashedKey(value);
            ori = ori.Replace('/', '_');
            ori = ori.Replace('-', '+');
            return ori;
        }
    }
}
