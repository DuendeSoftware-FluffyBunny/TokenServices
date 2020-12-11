using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OIDCConsentOrchestrator.Models
{
    public static class Constants
    {
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
            public const string Subject = "subject";
            public const string SubjectAndScopes = "subject_and_scopes";
        }
    }
}
