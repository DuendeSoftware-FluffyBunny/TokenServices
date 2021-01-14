using FluffyBunny4.DotNetCore.Collections;
using FluffyBunny4.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Stores.Serialization;
using FluffyBunny4.Services;

namespace FluffyBunny4.Stores
{

    public class MyDefaultRefreshTokenStore :
        DefaultGrantStore<RefreshTokenExtra>, IRefreshTokenStore, IGrantStoreHashAccessor
    {
        private IHashFixer _hashFixer;
        private IBackgroundTaskQueue<Delete> _taskQueueDelete;
        private IBackgroundTaskQueue<Write> _taskQueueWrite;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRefreshTokenStore"/> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="handleGenerationService">The handle generation service.</param>
        /// <param name="logger">The logger.</param>
        public MyDefaultRefreshTokenStore(
            IHashFixer hashFixer,
            IPersistedGrantStore store,
            IPersistentGrantSerializer serializer,
            IHandleGenerationService handleGenerationService,
            IBackgroundTaskQueue<Delete> taskQueueDelete,
            IBackgroundTaskQueue<Write> taskQueueWrite,
            ILogger<DefaultRefreshTokenStore> logger)
            : base(IdentityServerConstants.PersistedGrantTypes.RefreshToken, store, serializer, handleGenerationService, logger)
        {
            _hashFixer = hashFixer;
            _taskQueueDelete = taskQueueDelete;
            _taskQueueWrite = taskQueueWrite;
        }

        /// <summary>
        /// Stores the refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns></returns>
        public async Task<string> StoreRefreshTokenAsync(RefreshToken refreshToken)
        {
            var token = await CreateItemAsync(refreshToken as RefreshTokenExtra, 
                refreshToken.ClientId, 
                refreshToken.SubjectId, 
                refreshToken.SessionId, 
                refreshToken.Description, 
                refreshToken.CreationTime, 
                refreshToken.Lifetime);
            return token;
        }

        /// <summary>
        /// Updates the refresh token.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns></returns>
        public Task UpdateRefreshTokenAsync(string handle, RefreshToken refreshToken)
        {
            _taskQueueWrite.QueueBackgroundWorkItem(x =>
            {
                return StoreItemAsync(handle, refreshToken as RefreshTokenExtra, refreshToken.ClientId, refreshToken.SubjectId, refreshToken.SessionId, refreshToken.Description, refreshToken.CreationTime, refreshToken.CreationTime.AddSeconds(refreshToken.Lifetime), refreshToken.ConsumedTime);
            });
            return Task.CompletedTask;
         }

        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        /// <param name="refreshTokenHandle">The refresh token handle.</param>
        /// <returns></returns>
        public async Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenHandle)
        {
            return await GetItemAsync(refreshTokenHandle) as RefreshToken;
        }

        protected override Task<RefreshTokenExtra> GetItemAsync(string key)
        {
            var subKey = key.Substring(2);
            return base.GetItemAsync(subKey);
        }

        /// <summary>
        /// Removes the refresh token.
        /// </summary>
        /// <param name="refreshTokenHandle">The refresh token handle.</param>
        /// <returns></returns>
        public Task RemoveRefreshTokenAsync(string refreshTokenHandle)
        {
            DeferRemoveItemAsync(refreshTokenHandle);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes the refresh tokens.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public Task RemoveRefreshTokensAsync(string subjectId, string clientId)
        {
            DeferRemoveRefreshTokensAsync(subjectId, clientId);
            return Task.CompletedTask;
        }
        protected override string GetHashedKey(string value)
        {
            var hash = base.GetHashedKey(value);
            return _hashFixer.FixHash(hash);
        }
        string IGrantStoreHashAccessor.GetHashedKey(string value)
        {
            return GetHashedKey(value);
        }
        private void DeferRemoveRefreshTokensAsync(string subjectId, string clientId)
        {
            _taskQueueDelete.QueueBackgroundWorkItem(x =>
            {
                return RemoveAllAsync(subjectId, clientId);
            });
        }
        private void DeferRemoveItemAsync(string refresh_token)
        {
            _taskQueueDelete.QueueBackgroundWorkItem(x =>
            {
                return RemoveItemAsync(refresh_token);
            });
        }

       

        public class Delete
        {
        }

        public class Write
        {
        }
    }
}
