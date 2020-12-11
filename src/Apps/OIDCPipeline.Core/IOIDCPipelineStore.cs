using OIDCPipeline.Core.Endpoints.ResponseHandling;
using OIDCPipeline.Core.Validation.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public interface IOIDCPipelineStore
    {
        Task StoreTempCustomObjectAsync(string key, string subKey, object obj);
        Task<Dictionary<string, object>> GetTempCustomObjectsAsync(string key);
        Task StoreOriginalAuthorizeRequestAsync(string key, ValidatedAuthorizeRequest request);
        Task<ValidatedAuthorizeRequest> GetOriginalIdTokenRequestAsync(string key);
        Task StoreDownstreamIdTokenResponseAsync(string key, DownstreamAuthorizeResponse response);
        Task StoreDownstreamCustomDataAsync(string key, Dictionary<string,object> custom);
        Task<DownstreamAuthorizeResponse> GetDownstreamIdTokenResponseAsync(string key);
        Task DeleteStoredCacheAsync(string key);
    }
}
