using System;
using System.Collections.Generic;
using System.Text;

namespace FluffyBunny4
{
    public static class Constants
    {
        public const string ReservedSubject = "e461a271-1ec7-42aa-b6d3-915d8865ee5b";
        public const string ArbitraryClaims = "arbitrary_claims";
        public const string ArbitraryAmrs = "arbitrary_amrs";
        public const string ArbitraryAudiences = "arbitrary_audiences";
        public const string Scope = "scope";
        public const string CustomPayload = "custom_payload";
        public const string AccessTokenLifetime = "access_token_lifetime";
        public const string AccessTokenType = "access_token_type";

        public static class Discovery
        {
            public const string AuthorizationEndpoint = "authorization_endpoint";
            public const string AuthorizationType = "authorization_type";
            public const string ScopesSupported = "scopes_supported";
            public const string DiscoveryEndpoint = ".well-known/consent-configuration";
        }
        public static class AuthorizationTypes
        {
            public const string Implicit = "implicit";
            public const string SubjectAndScopes = "subject_and_scopes";
        }
        public static class TokenTypeHints
        {
            public const string RefreshToken = "refresh_token";
            public const string AccessToken = "access_token";
            public const string Subject = "subject";
        }
        public static class TokenExchangeTypes
        {
            public const string AccessToken = "urn:ietf:params:oauth:token-type:access_token";
            public const string RefreshToken = "urn:ietf:params:oauth:token-type:refresh_token";
            public const string IdToken = "urn:ietf:params:oauth:token-type:id_token";
            public const string SubjectToken  = "subject_token";
            public const string SubjectTokenType = "subject_token_type";
            
        }

        public static class ExternalServiceClient
        {
            public const string HttpClientName = "ExternalServiceHttpClient";
        }

        public static List<string> SupportedTokenTypeHints = new List<string>
        {
            TokenTypeHints.RefreshToken,
            TokenTypeHints.AccessToken,
            TokenTypeHints.Subject,
        };
        public static class RevocationErrors
        {
            public const string UnsupportedTokenType = "unsupported_token_type";
        }
        public class GrantType
        {
            public const string ArbitraryToken = "arbitrary_token";
            public const string TokenExchange = "urn:ietf:params:oauth:grant-type:token-exchange";
        }

        public static class EndpointNames
        {
            public const string Authorize = "Authorize";
            public const string Token = "Token";
            public const string DeviceAuthorization = "DeviceAuthorization";
            public const string Discovery = "Discovery";
            public const string Introspection = "Introspection";
            public const string Revocation = "Revocation";
            public const string EndSession = "Endsession";
            public const string CheckSession = "Checksession";
            public const string UserInfo = "Userinfo";
        }
        public static class ProtocolRoutePaths
        {
            public const string ConnectPathPrefix = "connect";

            public const string Authorize = ConnectPathPrefix + "/authorize";
            public const string AuthorizeCallback = Authorize + "/callback";
            public const string DiscoveryConfiguration = ".well-known/openid-configuration";
            public const string DiscoveryWebKeys = DiscoveryConfiguration + "/jwks";
            public const string Token = ConnectPathPrefix + "/token";
            public const string Revocation = ConnectPathPrefix + "/revocation";
            public const string UserInfo = ConnectPathPrefix + "/userinfo";
            public const string Introspection = ConnectPathPrefix + "/introspect";
            public const string EndSession = ConnectPathPrefix + "/endsession";
            public const string EndSessionCallback = EndSession + "/callback";
            public const string CheckSession = ConnectPathPrefix + "/checksession";
            public const string DeviceAuthorization = ConnectPathPrefix + "/deviceauthorization";

            public const string MtlsPathPrefix = ConnectPathPrefix + "/mtls";
            public const string MtlsToken = MtlsPathPrefix + "/token";
            public const string MtlsRevocation = MtlsPathPrefix + "/revocation";
            public const string MtlsIntrospection = MtlsPathPrefix + "/introspect";
            public const string MtlsDeviceAuthorization = MtlsPathPrefix + "/deviceauthorization";

            public static readonly string[] CorsPaths =
            {
                DiscoveryConfiguration,
                DiscoveryWebKeys,
                Token,
                UserInfo,
                Revocation
            };
        }
    }
}
