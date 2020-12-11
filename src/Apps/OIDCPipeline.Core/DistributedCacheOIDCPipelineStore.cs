using Common;
using FluffyBunny4.DotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
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
    internal class DistributedCacheOIDCPipelineStore : IOIDCPipelineStore
    {

        private IBinarySerializer _binarySerializer;
        private IDistributedCache _cache;
        private MemoryCacheOIDCPipelineStoreOptions _options;

        public DistributedCacheOIDCPipelineStore(
            IOptions<MemoryCacheOIDCPipelineStoreOptions> options,
            IBinarySerializer binarySerializer,
            IDistributedCache cache)
        {
            _binarySerializer = binarySerializer;
            _cache = cache;
            _options = options.Value;
        }

        public async Task DeleteStoredCacheAsync(string id)
        {
            var keyOriginal = OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id);
            var keyDownstream = OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id);
            await _cache.RemoveAsync(keyOriginal);
            await _cache.RemoveAsync(keyDownstream);

        }

        public async Task<DownstreamAuthorizeResponse> GetDownstreamIdTokenResponseAsync(string id)
        {
            var key = OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id);
            var result = await _cache.GetAsync(key);
            return _binarySerializer.Deserialize<DownstreamAuthorizeResponse>(result);
        }

        public async Task<ValidatedAuthorizeRequest> GetOriginalIdTokenRequestAsync(string id)
        {
            var key = OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id);
            var result = await _cache.GetAsync(key);
            return _binarySerializer.Deserialize<ValidatedAuthorizeRequest>(result);
        }

       

        public async Task StoreDownstreamCustomDataAsync(string id, Dictionary<string, object> custom)
        {
            var key = OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id);
            var result = await _cache.GetAsync(key);
            var value = _binarySerializer.Deserialize<DownstreamAuthorizeResponse>(result);

            if (value == null)
            {
                throw new Exception("Does not exist");
            }
            value.Custom = custom;
            await StoreDownstreamIdTokenResponseAsync(id, value);
        }

        public async Task StoreDownstreamIdTokenResponseAsync(string id, DownstreamAuthorizeResponse response)
        {
            var key = OIDCPipleLineStoreUtils.GenerateDownstreamIdTokenResponseKey(id);
            var data = _binarySerializer.Serialize(response);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.ExpirationMinutes)
            };
            await _cache.SetAsync(key, data, options);

        }

        public async Task StoreOriginalAuthorizeRequestAsync(string id, ValidatedAuthorizeRequest request)
        {
            var key = OIDCPipleLineStoreUtils.GenerateOriginalIdTokenRequestKey(id);

            var data = _binarySerializer.Serialize(request);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.ExpirationMinutes)
            };
            await _cache.SetAsync(key, data, options);
        }

        public async Task<Dictionary<string, object>> GetTempCustomObjectsAsync(string key)
        {
            var tempKey = $"{key}-tempCustom";
            var result = await _cache.GetAsync(tempKey);
            Dictionary<string, object> value = null;
            if (result == null)
            {
                value = new Dictionary<string, object>();
            }
            else
            {
                value = _binarySerializer.Deserialize<Dictionary<string, object>>(result);
            }

            return value;
        }

        public async Task StoreTempCustomObjectAsync(string key, string subKey, object obj)
        {
            var tempKey = $"{key}-tempCustom";
            Dictionary<string, object> value = await GetTempCustomObjectsAsync(key);

            value[subKey] = obj;
            var data = _binarySerializer.Serialize(value);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.ExpirationMinutes)
            };
            await _cache.SetAsync(tempKey, data, options);

        }
    }
}
