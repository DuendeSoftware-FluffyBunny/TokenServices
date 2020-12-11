using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppExternalLogin.Models;

namespace WebAppExternalLogin.Extensions
{
    public static class InMemoryIdentityServiceCollectionExtensions
    {
        public static IdentityBuilder AddAuthentication<TUser>(this IServiceCollection services, IConfiguration configuration)
            where TUser : class => services.AddAuthentication<TUser>(configuration, null);

        public static IdentityBuilder AddAuthentication<TUser>(this IServiceCollection services,
            IConfiguration configuration,
            Action<IdentityOptions> setupAction)
            where TUser : class
        {
            // Services used by identity
            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            });

            var section = configuration.GetSection("externalOIDC");
            var oAuth2SchemeRecords = new List<OpenIdConnectSchemeRecord>();
            section.Bind(oAuth2SchemeRecords);
            services.AddSingleton(oAuth2SchemeRecords);
            foreach (var record in oAuth2SchemeRecords)
            {
                var scheme = record.Scheme;
                authenticationBuilder.AddOpenIdConnect(scheme, scheme, options =>
                {
                    options.TokenValidationParameters.ValidateAudience = false;
                    options.TokenValidationParameters.ValidateIssuer = false;
                    options.Authority = record.Authority;
                    options.CallbackPath = record.CallbackPath;
                    options.RequireHttpsMetadata = false;
                    if (!string.IsNullOrEmpty(record.ResponseType))
                    {
                        options.ResponseType = record.ResponseType;
                    }
                    options.GetClaimsFromUserInfoEndpoint = record.GetClaimsFromUserInfoEndpoint;
                    options.ClientId = record.ClientId;
                    options.ClientSecret = record.ClientSecret;
                    options.SaveTokens = true;

                    options.Events.OnMessageReceived = context =>
                    {
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        var query = from item in context.Request.Query
                                    where string.Compare(item.Key, "prompt", true) == 0
                                    select item.Value;
                        if (query.Any())
                        {
                            var prompt = query.FirstOrDefault();
                            context.ProtocolMessage.Prompt = prompt;
                        }

                        if (record.AdditionalProtocolScopes != null && record.AdditionalProtocolScopes.Any())
                        {
                            string additionalScopes = "";
                            foreach (var item in record.AdditionalProtocolScopes)
                            {
                                additionalScopes += $" {item}";
                            }
                            context.ProtocolMessage.Scope += additionalScopes;
                        }
                        if (context.HttpContext.User.Identity.IsAuthenticated)
                        {
                            // assuming a relogin trigger, so we will make the user relogin on the IDP
                            context.ProtocolMessage.Prompt = "login";
                        }
                        context.ProtocolMessage.SetParameter("idp_code", "DT");
                        context.ProtocolMessage.SetParameter("custom", "{\"someCustomJson\":\"hi\"}");
                        /*
                        if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                        {
                            context.ProtocolMessage.AcrValues = "v1=google";
                        }
                        */
                        return Task.CompletedTask;
                    };
                    options.Events.OnRemoteFailure = context =>
                    {
                        context.Response.Redirect("/");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    };
                });
            }

            // claims transformation is run after every Authenticate call
            return new IdentityBuilder(typeof(TUser), services);
        }
    }
}
