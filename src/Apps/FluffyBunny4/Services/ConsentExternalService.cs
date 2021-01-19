using FluffyBunny4.Models;
using FluffyBunny4.Models.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluffyBunny4.Configuration;
using Microsoft.Extensions.Options;

namespace FluffyBunny4.Services
{
    public class ConsentExternalService : IConsentExternalService
    {
        private ExternalServicesOptions _options;
        private IHttpClientFactory _httpClientFactory;
        private ILogger<ConsentExternalService> _logger;

        public ConsentExternalService(
            IOptions<ExternalServicesOptions> options,
            IHttpClientFactory httpClientFactory, 
            ILogger<ConsentExternalService> logger)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        private HttpClient GetHttpClient()
        {
            return _httpClientFactory.CreateClient(FluffyBunny4.Constants.ExternalServiceClient.HttpClientName);
        }
        private static async Task<HttpResponseMessage> PostJsonContentAsync<T>(string uri, HttpClient httpClient, T obj, CancellationToken cancellationToken)
        {

            var postRequest = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = JsonContent.Create(obj)
            };

            var postResponse = await httpClient.SendAsync(postRequest, cancellationToken);

            // postResponse.EnsureSuccessStatusCode();

            return postResponse;
        }

        public async Task<ConsentAuthorizeResponseContainer<T>> PostAuthorizationRequestAsync<T>(
            ConsentDiscoveryDocumentResponse discovery,
            ConsentAuthorizeRequest requestObject,
            T context) where T: class
        {
            var response = new ConsentAuthorizeResponseContainer<T> {Context = context};
            try
            {
                var s_cts = new CancellationTokenSource(); 
                s_cts.CancelAfter(_options.RequestTimeout);
                var httpClient = GetHttpClient();
                using var httpResponse = await PostJsonContentAsync(discovery.AuthorizeEndpoint, httpClient, requestObject, s_cts.Token);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    response.Response = new ConsentAuthorizeResponse()
                    {
                        Subject = requestObject.Subject,
                        Scopes = requestObject.Scopes,
                        Authorized = false,
                        Error = new ConsentBaseResponse.ConsentError
                        {
                            Message = $"StatusCode={httpResponse.StatusCode}",
                            StatusCode = (int)httpResponse.StatusCode
                        }
                    };
                    if (httpResponse.Content is object)
                    {
                        var contentText = await httpResponse.Content.ReadAsStringAsync();
                        response.Response.Error.Message = contentText;
                    }
                    _logger.LogError($"authorizationEndPoint={discovery.AuthorizeEndpoint},statusCode={httpResponse.StatusCode},content=\'{response.Response.Error.Message}\'");
                    return response;
                }


                if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
                {
                    var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                    var consentAuthorizeResponse = await System.Text.Json.JsonSerializer.DeserializeAsync<ConsentAuthorizeResponse>(contentStream, new System.Text.Json.JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true });
                    response.Response = consentAuthorizeResponse;
                    return response;
                }
                throw new Exception("HTTP Response was invalid and cannot be deserialized.");

            }
            catch (Exception ex)
            {
                response.Response = new ConsentAuthorizeResponse()
                {
                    Subject = requestObject.Subject,
                    Scopes = requestObject.Scopes,
                    Authorized = false,
                    Error = new ConsentBaseResponse.ConsentError
                    {
                        Message = ex.Message,
                        StatusCode = (int)HttpStatusCode.BadRequest
                    }
                };
                return response;
            }
        }
    }
}
