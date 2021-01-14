using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Stores.Serialization;
using FluffyBunny4.Services;
using Microsoft.Extensions.Logging;

namespace FluffyBunny4.Stores
{
    // Only need this because the cosmos ids have chars that are excluded.
    public class MyDefaultReferenceTokenStore : DefaultReferenceTokenStore, IGrantStoreHashAccessor
    {
        private IHashFixer _hashFixer;

        public MyDefaultReferenceTokenStore(
            IHashFixer hashFixer,
            IPersistedGrantStore store, 
            IPersistentGrantSerializer serializer, 
            IHandleGenerationService handleGenerationService, 
            ILogger<DefaultReferenceTokenStore> logger) : 
            base(store, serializer, handleGenerationService, logger)
        {
            _hashFixer = hashFixer;
        }
        protected override string GetHashedKey(string value)
        {
            // COSMOS
            // >> The following characters are restricted and cannot be used in the Id property: '/', '\\', '?', '#'
            var hash = base.GetHashedKey(value);
            return _hashFixer.FixHash(hash);
           
        }

        string IGrantStoreHashAccessor.GetHashedKey(string value)
        {
            return GetHashedKey(value);
        }

        protected override Task<Token> GetItemAsync(string key)
        {
            var subKey = key.Substring(2);
            return base.GetItemAsync(subKey);
        }
 
    }
}
