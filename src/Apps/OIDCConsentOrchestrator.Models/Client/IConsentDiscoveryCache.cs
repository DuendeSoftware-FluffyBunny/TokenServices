using System;
using System.Threading.Tasks;
namespace OIDCConsentOrchestrator.Models.Client
{
    
    /// <summary>
    /// Interface for discovery cache
    /// </summary>
    public interface IConsentDiscoveryCache
    {
        /// <summary>
        /// Gets or sets the duration of the cache.
        /// </summary>
        /// <value>
        /// The duration of the cache.
        /// </value>
        TimeSpan CacheDuration { get; set; }

        /// <summary>
        /// Retrieves the discovery document
        /// </summary>
        /// <returns></returns>
        Task<ConsentDiscoveryDocumentResponse> GetAsync();

        /// <summary>
        /// Forces a refresh on the next get.
        /// </summary>
        void Refresh();
    }
}