using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using FluffyBunny4.Models.Client;

namespace FluffyBunny4.Cache
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
