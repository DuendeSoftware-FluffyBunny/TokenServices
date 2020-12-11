using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OIDCPipeline.Core.Extensions;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace OIDCPipeline.Core.Validation.Models
{
    internal class AuthorizeResult : IActionResult
    {
        public AuthorizeResponse Response { get; }

        public AuthorizeResult(AuthorizeResponse response)
        {
            Response = response ?? throw new ArgumentNullException(nameof(response));
        }

        private void Init(HttpContext context)
        {

        }

        public async Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            if (Response.IsError)
            {
                await ProcessErrorAsync(context);
            }
            else
            {
                await ProcessResponseAsync(context);
            }
        }

        private async Task ProcessErrorAsync(HttpContext context)
        {
            // these are the conditions where we can send a response 
            // back directly to the client, otherwise we're only showing the error UI
            var isPromptNoneError = Response.Error == OidcConstants.AuthorizeErrors.AccountSelectionRequired ||
                Response.Error == OidcConstants.AuthorizeErrors.LoginRequired ||
                Response.Error == OidcConstants.AuthorizeErrors.ConsentRequired ||
                Response.Error == OidcConstants.AuthorizeErrors.InteractionRequired;

            if (Response.Error == OidcConstants.AuthorizeErrors.AccessDenied ||
                isPromptNoneError && Response.Request?.PromptMode == OidcConstants.PromptModes.None
            )
            {
                // this scenario we can return back to the client
                await ProcessResponseAsync(context);
            }
            else
            {
                // we now know we must show error page
                //             await RedirectToErrorPageAsync(context);
            }
        }

        protected async Task ProcessResponseAsync(HttpContext context)
        {


            await RenderAuthorizeResponseAsync(context);
        }

        private async Task RenderAuthorizeResponseAsync(HttpContext context)
        {
            if (Response.Request.ResponseMode == OidcConstants.ResponseModes.Query ||
                Response.Request.ResponseMode == OidcConstants.ResponseModes.Fragment)
            {
                context.Response.SetNoCache();
                context.Response.Redirect(BuildRedirectUri());
            }
            else if (Response.Request.ResponseMode == OidcConstants.ResponseModes.FormPost)
            {
                context.Response.SetNoCache();
                AddSecurityHeaders(context);
                await context.Response.WriteHtmlAsync(GetFormPostHtml());
            }
            else
            {
                //_logger.LogError("Unsupported response mode.");
                throw new InvalidOperationException("Unsupported response mode");
            }
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            //     context.Response.AddScriptCspHeaders(_options.Csp, "sha256-orD0/VhH8hLqrLxKHD/HUEMdwqX6/0ve7c5hspX5VJ8=");
            context.Response.AddScriptCspHeaders(new CspOptions(),
                "sha256-orD0/VhH8hLqrLxKHD/HUEMdwqX6/0ve7c5hspX5VJ8=");

            var referrer_policy = "no-referrer";
            if (!context.Response.Headers.ContainsKey("Referrer-Policy"))
            {
                context.Response.Headers.Add("Referrer-Policy", referrer_policy);
            }
        }

        private string BuildRedirectUri()
        {
            var uri = Response.RedirectUri;
            var finalResponse = Response.ToNameValueCollection();
            if (Response.Request.GrantType != GrantType.AuthorizationCode)
            {
                // we want to give out our extra custom stuff here as its the final redirect back
                finalResponse.Merge(Response.Downstream.Custom);
                // if is the AuthorizationCode its picked up in a subsequent call to the token endpoint
            }
            
            var query = finalResponse.ToQueryString();

            if (Response.Request.ResponseMode == OidcConstants.ResponseModes.Query)
            {
                uri = uri.AddQueryString(query);
            }
            else
            {
                uri = uri.AddHashFragment(query);
            }

            if (Response.IsError && !uri.Contains("#"))
            {
                // https://tools.ietf.org/html/draft-bradley-oauth-open-redirector-00
                uri += "#_=_";
            }

            return uri;
        }

        private const string FormPostHtml = "<html><head><base target='_self'/></head><body><form method='post' action='{uri}'>{body}<noscript><button>Click to continue</button></noscript></form><script>window.addEventListener('load', function(){document.forms[0].submit();});</script></body></html>";

        private string GetFormPostHtml()
        {
            var html = FormPostHtml;

            var url = Response.Request.RedirectUri;
            url = HtmlEncoder.Default.Encode(url);
            html = html.Replace("{uri}", url);
            var finalResponse = Response.ToNameValueCollection();
            if (Response.Request.GrantType != GrantType.AuthorizationCode)
            {
                // we want to give out our extra custom stuff here as its the final redirect back
                finalResponse.Merge(Response.Downstream.Custom);
                // if is the AuthorizationCode its picked up in a subsequent call to the token endpoint
            }
         
            html = html.Replace("{body}", finalResponse.ToFormPost());

            return html;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            await ExecuteAsync(context.HttpContext);
        }
    }
}
