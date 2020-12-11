using IdentityModel;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIDCPipeline.Core
{
    internal class Constants
    {
        public static readonly List<string> AllowedGrantTypesForAuthorizeEndpoint = new List<string>
        {
            GrantType.AuthorizationCode,
            GrantType.Implicit,
            GrantType.Hybrid
        };
        public static readonly Dictionary<string, IEnumerable<string>> AllowedResponseModesForGrantType = new Dictionary<string, IEnumerable<string>>
        {
            { GrantType.AuthorizationCode, new[] { OidcConstants.ResponseModes.Query, OidcConstants.ResponseModes.FormPost } },
            { GrantType.Hybrid, new[] { OidcConstants.ResponseModes.Fragment, OidcConstants.ResponseModes.FormPost }},
            { GrantType.Implicit, new[] { OidcConstants.ResponseModes.Fragment, OidcConstants.ResponseModes.FormPost }}
        };
        public static readonly List<string> SupportedResponseModes = new List<string>
        {
            OidcConstants.ResponseModes.FormPost,
            OidcConstants.ResponseModes.Query 
        };
        public static readonly List<string> SupportedCodeChallengeMethods = new List<string>
        {
            OidcConstants.CodeChallengeMethods.Plain,
            OidcConstants.CodeChallengeMethods.Sha256
        };
        public static readonly Dictionary<string, string> ResponseTypeToGrantTypeMapping = new Dictionary<string, string>
        {
            { OidcConstants.ResponseTypes.Code, GrantType.AuthorizationCode },
            { OidcConstants.ResponseTypes.Token, GrantType.Implicit },
            { OidcConstants.ResponseTypes.IdToken, GrantType.Implicit },
            { OidcConstants.ResponseTypes.IdTokenToken, GrantType.Implicit },
            { OidcConstants.ResponseTypes.CodeIdToken, GrantType.Hybrid },
            { OidcConstants.ResponseTypes.CodeToken, GrantType.Hybrid },
            { OidcConstants.ResponseTypes.CodeIdTokenToken, GrantType.Hybrid }
        };
        public static class EndpointNames
        {
            public const string Authorize = "Authorize";
            public const string Token = "Token";
            public const string Discovery = "Discovery";
        }
        public static class ProtocolRoutePaths
        {
            public const string ConnectPathPrefix = "connect";

            public const string Authorize = ConnectPathPrefix + "/authorize";
            
            public const string DiscoveryConfiguration = ".well-known/openid-configuration";
        
            public const string Token = ConnectPathPrefix + "/token";


            public static readonly string[] CorsPaths =
            {
                DiscoveryConfiguration,
              
                Token,
           
            };
        }
    }
}
