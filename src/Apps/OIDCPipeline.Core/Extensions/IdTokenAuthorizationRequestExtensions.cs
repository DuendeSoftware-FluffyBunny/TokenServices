using FluffyBunny4.DotNetCore;
using IdentityModel;
using OIDCPipeline.Core.Endpoints.ResponseHandling;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace OIDCPipeline.Core.Extensions
{
    internal static class IdTokenAuthorizationRequestExtensions
    {
        static List<string> WellKnownOIDCValues = new List<string> {
            OidcConstants.AuthorizeRequest.ClientId,
            "client_secret",
            OidcConstants.AuthorizeRequest.Nonce,
            OidcConstants.AuthorizeRequest.ResponseMode,
            OidcConstants.AuthorizeRequest.RedirectUri,
            OidcConstants.AuthorizeRequest.ResponseType,
            OidcConstants.AuthorizeRequest.State,
            OidcConstants.AuthorizeRequest.Scope
        };
        public static DownstreamAuthorizationRequest ToDownstreamAuthorizationRequest(this SimpleNameValueCollection values)
        {
            var extraNames = new List<string>();
            foreach (var item in values.AllKeys)
            {
                if (!WellKnownOIDCValues.Contains(item))
                {
                    extraNames.Add(item);
                }
            }
            var idTokenAuthorizationRequest = new DownstreamAuthorizationRequest
            {
                client_id = values.Get(OidcConstants.AuthorizeRequest.ClientId),
                client_secret = values.Get("client_secret"),
                nonce = values.Get(OidcConstants.AuthorizeRequest.Nonce),
                response_mode = values.Get(OidcConstants.AuthorizeRequest.ResponseMode),
                redirect_uri = values.Get(OidcConstants.AuthorizeRequest.RedirectUri),
                response_type = values.Get(OidcConstants.AuthorizeRequest.ResponseType),
                state = values.Get(OidcConstants.AuthorizeRequest.State),
                scope = values.Get(OidcConstants.AuthorizeRequest.Scope)
               
            };
            if (extraNames.Any())
            {
                idTokenAuthorizationRequest.ExtraValues = new NameValueCollection();
            }
            foreach(var extraName in extraNames)
            {
                idTokenAuthorizationRequest.ExtraValues.Set(extraName,values.Get(extraName));
            }
            return idTokenAuthorizationRequest;
        }
    }
}
