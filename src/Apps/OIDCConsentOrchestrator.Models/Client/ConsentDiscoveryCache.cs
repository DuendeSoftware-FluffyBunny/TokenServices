using System;
using System.Net.Http;
using System.Threading.Tasks;
using OIDCConsentOrchestrator.Models.Internal;
namespace OIDCConsentOrchestrator.Models.Client
{
    /// <summary>
    /// Helper for caching discovery documents.
    /// </summary>
    public class ConsentDiscoveryCache : IConsentDiscoveryCache
    {
        private DateTime _nextReload = DateTime.MinValue;
        private AsyncLazy<ConsentDiscoveryDocumentResponse> _lazyResponse;

      
        private readonly Func<HttpMessageInvoker> _getHttpClient;
        private readonly string _authority;

        /// <summary>
        /// Initialize instance of DiscoveryCache with passed authority.
        /// </summary>
        /// <param name="authority">Base address or discovery document endpoint.</param>
        /// <param name="policy">The policy.</param>
        public ConsentDiscoveryCache(string authority)
        {
            _authority = authority;
            _getHttpClient = () => new HttpClient();
        }

        /// <summary>
        /// Initialize instance of DiscoveryCache with passed authority.
        /// </summary>
        /// <param name="authority">Base address or discovery document endpoint.</param>
        /// <param name="httpClientFunc">The HTTP client function.</param>
        /// <param name="policy">The policy.</param>
        public ConsentDiscoveryCache(string authority, Func<HttpMessageInvoker> httpClientFunc)
        {
            _authority = authority;
            _getHttpClient = httpClientFunc ?? throw new ArgumentNullException(nameof(httpClientFunc));
        }

        /// <summary>
        /// Frequency to refresh discovery document. Defaults to 24 hours.
        /// </summary>
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromHours(24);

        /// <summary>
        /// Get the DiscoveryResponse either from cache or from discovery endpoint.
        /// </summary>
        /// <returns></returns>
        public Task<ConsentDiscoveryDocumentResponse> GetAsync()
        {
            if (_nextReload <= DateTime.UtcNow)
            {
                Refresh();
            }

            return _lazyResponse.Value;
        }

        /// <summary>
        /// Marks the discovery document as stale and will trigger a request to the discovery endpoint on the next request to get the DiscoveryResponse.
        /// </summary>
        public void Refresh()
        {
            _lazyResponse = new OIDCConsentOrchestrator.Models.Internal.AsyncLazy<ConsentDiscoveryDocumentResponse>(GetResponseAsync);
        }

        private async Task<ConsentDiscoveryDocumentResponse> GetResponseAsync()
        {
            var result = await _getHttpClient().GetDiscoveryDocumentAsync(new ConsentDiscoveryDocumentRequest
            {
                Address = _authority 
            }).ConfigureAwait();

            if (result.IsError)
            {
                Refresh();
                _nextReload = DateTime.MinValue;
            }
            else
            {
                _nextReload = DateTime.UtcNow.Add(CacheDuration);
            }

            return result;
        }
    }
}