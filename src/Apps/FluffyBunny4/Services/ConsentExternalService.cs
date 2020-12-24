using FluffyBunny4.Models;
using FluffyBunny4.Models.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FluffyBunny4.Services
{
    public class ConsentExternalService : IConsentExternalService
    {
        private IHttpClientFactory _httpClientFactory;
        private ILogger<ConsentExternalService> _logger;

        public ConsentExternalService(IHttpClientFactory httpClientFactory, ILogger<ConsentExternalService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        private HttpClient GetHttpClient()
        {
            return _httpClientFactory.CreateClient(FluffyBunny4.Constants.ExternalServiceClient.HttpClientName);
        }
        private static async Task<HttpResponseMessage> PostJsonContentAsync<T>(string uri, HttpClient httpClient, T obj)
        {

            var postRequest = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = JsonContent.Create(obj)
            };

            var postResponse = await httpClient.SendAsync(postRequest);

            // postResponse.EnsureSuccessStatusCode();

            return postResponse;
        }

        public async Task<ConsentAuthorizeResponse> PostAuthorizationRequestAsync(
            ConsentDiscoveryDocumentResponse discovery,
            ConsentAuthorizeRequest requestObject)
        {
            try
            {
                var httpClient = GetHttpClient();
                using var httpResponse = await PostJsonContentAsync(discovery.AuthorizeEndpoint, httpClient, requestObject);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var result = new ConsentAuthorizeResponse()
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
                        result.Error.Message = contentText;
                    }
                    _logger.LogError($"authorizationEndPoint={discovery.AuthorizeEndpoint},statusCode={httpResponse.StatusCode},content=\'{result.Error.Message}\'");
                    return result;
                }


                if (httpResponse.Content is object && httpResponse.Content.Headers.ContentType.MediaType == "application/json")
                {
                    var contentStream = await httpResponse.Content.ReadAsStreamAsync();

                    var consentAuthorizeResponse = await System.Text.Json.JsonSerializer.DeserializeAsync<ConsentAuthorizeResponse>(contentStream, new System.Text.Json.JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true });
                    return consentAuthorizeResponse;
                }
                throw new Exception("HTTP Response was invalid and cannot be deserialized.");

            }
            catch (Exception ex)
            {
                var result = new ConsentAuthorizeResponse()
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
                return result;
            }
        }
    }
}
