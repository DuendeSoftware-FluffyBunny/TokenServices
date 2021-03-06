﻿using FluffyBunny4.Cache;
using FluffyBunny4.Extensions;
using FluffyBunny4.Models.Client;
using FluffyBunny4.Models.Internal;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluffyBunny4.Services
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
        /// Frequency to refresh discovery document on error. Defaults to 1 minute.
        /// </summary>
        public TimeSpan CacheErrorDuration { get; set; } = TimeSpan.FromMinutes(1);

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
            _lazyResponse = new AsyncLazy<ConsentDiscoveryDocumentResponse>(GetResponseAsync);
        }

        private async Task<ConsentDiscoveryDocumentResponse> GetResponseAsync()
        {
            var result = await _getHttpClient().GetDiscoveryDocumentAsync(new ConsentDiscoveryDocumentRequest
            {
                Address = _authority
            }).ConfigureAwait();

            if (result.IsError)
            {
           //     Refresh();
                _nextReload = DateTime.UtcNow.Add(CacheErrorDuration);
            }
            else
            {
                _nextReload = DateTime.UtcNow.Add(CacheDuration);
            }

            return result;
        }
    }
}
