using System;
using System.Linq;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTest_json
{
    public class UnitTest1
    {
        private const string DiscoveryDocument =
            "{\r\n    \"issuer\": \"http://localhost:7000/zep\",\r\n    \"jwks_uri\": \"http://localhost:7000/zep/.well-known/openid-configuration/jwks\",\r\n    \"authorization_endpoint\": \"http://localhost:7000/zep/connect/authorize\",\r\n    \"token_endpoint\": \"http://localhost:7000/zep/connect/token\",\r\n    \"userinfo_endpoint\": \"http://localhost:7000/zep/connect/userinfo\",\r\n    \"end_session_endpoint\": \"http://localhost:7000/zep/connect/endsession\",\r\n    \"check_session_iframe\": \"http://localhost:7000/zep/connect/checksession\",\r\n    \"revocation_endpoint\": \"http://localhost:7000/zep/connect/revocation\",\r\n    \"introspection_endpoint\": \"http://localhost:7000/zep/connect/introspect\",\r\n    \"device_authorization_endpoint\": \"http://localhost:7000/zep/connect/deviceauthorization\",\r\n    \"frontchannel_logout_supported\": true,\r\n    \"frontchannel_logout_session_supported\": true,\r\n    \"backchannel_logout_supported\": true,\r\n    \"backchannel_logout_session_supported\": true,\r\n    \"scopes_supported\": [\r\n        \"offline_access\"\r\n    ],\r\n    \"claims_supported\": [],\r\n    \"grant_types_supported\": [\r\n        \"authorization_code\",\r\n        \"client_credentials\",\r\n        \"refresh_token\",\r\n        \"implicit\",\r\n        \"urn:ietf:params:oauth:grant-type:device_code\",\r\n        \"arbitrary_identity\",\r\n        \"arbitrary_token\",\r\n        \"urn:ietf:params:oauth:grant-type:token-exchange\",\r\n        \"urn:ietf:params:oauth:grant-type:token-exchange-mutate\"\r\n    ],\r\n    \"response_types_supported\": [\r\n        \"code\",\r\n        \"token\",\r\n        \"id_token\",\r\n        \"id_token token\",\r\n        \"code id_token\",\r\n        \"code token\",\r\n        \"code id_token token\"\r\n    ],\r\n    \"response_modes_supported\": [\r\n        \"form_post\",\r\n        \"query\",\r\n        \"fragment\"\r\n    ],\r\n    \"token_endpoint_auth_methods_supported\": [\r\n        \"client_secret_basic\",\r\n        \"client_secret_post\"\r\n    ],\r\n    \"subject_types_supported\": [\r\n        \"public\"\r\n    ],\r\n    \"code_challenge_methods_supported\": [\r\n        \"plain\",\r\n        \"S256\"\r\n    ],\r\n    \"request_parameter_supported\": true\r\n}";
        [Fact]
        public void Test1()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var disco = JsonSerializer.Deserialize<Duende.IdentityServer.Models.DiscoveryDocument>(DiscoveryDocument, options);
            disco.issuer = "https://accounts.google.com";


           
            var jsonDisco = JsonSerializer.Serialize(disco, options);

        }
    }
}
