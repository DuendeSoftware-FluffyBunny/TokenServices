using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OIDCPipeline.Core.Configuration;
using OIDCPipeline.Core.Endpoints.ResponseHandling;
using OIDCPipeline.Core.Validation.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    internal class MemoryCacheOIDCPipelineStore : IOIDCPipelineStore
    {
        private IMemoryCache _memoryCache;
        private MemoryCacheOIDCPipelineStoreOptions _options;

        public MemoryCacheOIDCPipelineStore(
            IOptions<MemoryCacheOIDCPipelineStoreOptions> options,
            IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _options = options.Value;
        }

        public Task DeleteStoredCacheAsync(string id)
        {
            var keyOriginal = OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id);
            var keyDownstream = OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id);
            _memoryCache.Remove(keyOriginal);
            _memoryCache.Remove(keyDownstream);
            return Task.CompletedTask;
        }

        public Task<DownstreamAuthorizeResponse> GetDownstreamIdTokenResponseAsync(string id)
        {
            var key = OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id);
            var result = _memoryCache.Get<DownstreamAuthorizeResponse>(key);
            return Task.FromResult(result);
        }

        public Task<ValidatedAuthorizeRequest> GetOriginalIdTokenRequestAsync(string id)
        {
            var key = OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id);
            var result = _memoryCache.Get<ValidatedAuthorizeRequest>(key);
            return Task.FromResult(result);
        }

        public Task StoreDownstreamCustomDataAsync(string id, Dictionary<string, object> custom)
        {
            var key = OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id);
            var result = _memoryCache.Get<DownstreamAuthorizeResponse>(key);
            if(result == null)
            {
                throw new Exception("Does not exist");
            }
            result.Custom = custom;
            return StoreDownstreamIdTokenResponseAsync(id, result);
        }

        public Task StoreDownstreamIdTokenResponseAsync(string id, DownstreamAuthorizeResponse response)
        {
            var key = OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id);
            _memoryCache.Set(
                key, 
                response, 
                TimeSpan.FromMinutes(_options.ExpirationMinutes));
            return Task.CompletedTask;
        }

        public Task StoreTempCustomObjectAsync(string key, string subKey, object obj)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, object>> GetTempCustomObjectsAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task StoreOriginalAuthorizeRequestAsync(string id, ValidatedAuthorizeRequest request)
        {
            var key = OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id);
            _memoryCache.Set(
                key, 
                request, 
                TimeSpan.FromMinutes(_options.ExpirationMinutes));
            return Task.CompletedTask;
        }
    }
}
