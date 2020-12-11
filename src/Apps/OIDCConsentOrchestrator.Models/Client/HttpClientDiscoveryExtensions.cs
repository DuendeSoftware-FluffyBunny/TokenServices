using OIDCConsentOrchestrator.Models.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
 
namespace OIDCConsentOrchestrator.Models.Client
{

    /// <summary>
    /// HttpClient extentions for OIDC discovery
    /// </summary>
    public static class HttpClientDiscoveryExtensions
    {
        /// <summary>
        /// Sends a discovery document request
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="address">The address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<ConsentDiscoveryDocumentResponse> GetDiscoveryDocumentAsync(this HttpClient client, string address = null, CancellationToken cancellationToken = default)
        {
            return await client.GetDiscoveryDocumentAsync(new ConsentDiscoveryDocumentRequest { Address = address }, cancellationToken).ConfigureAwait();
        }

        /// <summary>
        /// Sends a discovery document request
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async Task<ConsentDiscoveryDocumentResponse> GetDiscoveryDocumentAsync(this HttpMessageInvoker client, 
            ConsentDiscoveryDocumentRequest request, 
            CancellationToken cancellationToken = default)
        {
            string address;
            if (request.Address.IsPresent())
            {
                address = request.Address;
            }
            else if (client is HttpClient)
            {
                address = ((HttpClient)client).BaseAddress.AbsoluteUri;
            }
            else
            {
                throw new ArgumentException("An address is required.");
            }

            var parsed = ConsentDiscoveryEndpoint.ParseUrl(address);
            var authority = parsed.Authority;
            var url = parsed.Url;

            if (request.Policy.Authority.IsMissing())
            {
                request.Policy.Authority = authority;
            }

            string jwkUrl = "";

            if (!ConsentDiscoveryEndpoint.IsSecureScheme(new Uri(url), request.Policy))
            {
                return ConsentProtocolResponse.FromException<ConsentDiscoveryDocumentResponse>(new InvalidOperationException("HTTPS required"), $"Error connecting to {url}. HTTPS required.");
            }

            try
            {
                var clone = request.Clone();

                clone.Method = HttpMethod.Get;
                clone.Prepare();

                clone.RequestUri = new Uri(url);

                var response = await client.SendAsync(clone, cancellationToken).ConfigureAwait();

                string responseContent = null;

                if (response.Content != null)
                {
                    responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait();
                }

                if (!response.IsSuccessStatusCode)
                {
                    return await ConsentProtocolResponse.FromHttpResponseAsync<ConsentDiscoveryDocumentResponse>(response, $"Error connecting to {url}: {response.ReasonPhrase}").ConfigureAwait();
                }

                var disco = await ConsentProtocolResponse.FromHttpResponseAsync<ConsentDiscoveryDocumentResponse>(response, request.Policy).ConfigureAwait();

                return disco;
                
            }
            catch (Exception ex)
            {
                return ConsentProtocolResponse.FromException<ConsentDiscoveryDocumentResponse>(ex, $"Error connecting to {url}. {ex.Message}.");
            }
        }
    }
}
