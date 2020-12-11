using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OIDCPipeline.Core;
using OIDCPipeline.Core.Endpoints.ResponseHandling;
using OIDCPipeline.Core.Validators;
using OpenIdConnectModels;

namespace OIDCConsentOrchestrator.Authentication
{
    public static class OIDCAuthenticationExtensions
    {
        public static IdentityBuilder AddAuthentication<TUser>(
            this IServiceCollection services,
            List<OpenIdConnectSchemeRecord> oidcSchemeRecords)
            where TUser : class
        {
            // Services used by identity

            var authenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            });
 
 
            foreach (var record in oidcSchemeRecords)
            {
                var scheme = record.Scheme;
                services.AddOptions<OpenIdConnectOptions>(scheme)
                                     .Configure<IOIDCPipeLineKey, IOIDCPipelineStore>(
                    (options, oidcPipeLineKey, oidcPipelineStore) =>
                    {
                        options.ProtocolValidator = new OIDCPipelineOpenIdConnectProtocolValidator(oidcPipeLineKey, oidcPipelineStore)
                        {
                            RequireTimeStampInNonce = false,
                            RequireStateValidation = false,
                            RequireNonce = true,
                            NonceLifetime = TimeSpan.FromMinutes(15)
                        };
                    });


                authenticationBuilder.AddOpenIdConnect(scheme, scheme, options =>
                {
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
                    options.TokenValidationParameters.ValidateAudience = false;
                    options.Events.OnMessageReceived = async context =>
                    {

                    };
                    options.Events.OnTokenValidated = async context =>
                    {
                        var pipeLineStore = context.HttpContext.RequestServices.GetRequiredService<IOIDCPipelineStore>();

                        OpenIdConnectMessage oidcMessage = null;
                        if (context.Options.ResponseType == "id_token")
                        {
                            oidcMessage = context.ProtocolMessage;
                        }
                        else
                        {
                            oidcMessage = context.TokenEndpointResponse;
                        }

                        var userState = context.ProtocolMessage.Parameters["state"].Split('.')[0];

                //        var header = new JwtHeader();
                //        var handler = new JwtSecurityTokenHandler();
                //        var idToken = handler.ReadJwtToken(oidcMessage.IdToken);
               //         var claims = idToken.Claims.ToList();

                        var stored = await pipeLineStore.GetOriginalIdTokenRequestAsync(userState);

                        DownstreamAuthorizeResponse idTokenResponse = new DownstreamAuthorizeResponse
                        {
                            Request = stored,
                            AccessToken = oidcMessage.AccessToken,
                            ExpiresAt = oidcMessage.ExpiresIn,
                            IdToken = oidcMessage.IdToken,
                            RefreshToken = oidcMessage.RefreshToken,
                            TokenType = oidcMessage.TokenType,
                            LoginProvider = scheme
                        };
                        await pipeLineStore.StoreDownstreamIdTokenResponseAsync(userState, idTokenResponse);

                    };
                    options.Events.OnRedirectToIdentityProvider = async context =>
                    {
                        var oidcPipelineKey = context.HttpContext.RequestServices.GetRequiredService<IOIDCPipeLineKey>();

                        string key = oidcPipelineKey.GetOIDCPipeLineKey();
                        var pipeLineStore = context.HttpContext.RequestServices.GetRequiredService<IOIDCPipelineStore>();
                        var stored = await pipeLineStore.GetOriginalIdTokenRequestAsync(key);
                        var clientSecretStore = context.HttpContext.RequestServices.GetRequiredService<IOIDCPipelineClientStore>();

                        if (stored != null)
                        {
                            var clientRecord = await clientSecretStore.FetchClientRecordAsync(scheme,
                                stored.ClientId);
                            context.ProtocolMessage.ClientId = stored.ClientId;
                            context.Options.ClientId = stored.ClientId;
                            context.Options.ClientSecret = clientRecord.Secret;
                            context.ProtocolMessage.State = $"{key}.";
                        }

                        context.Options.Authority = context.Options.Authority;
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
                        var allowedParams = await clientSecretStore.FetchAllowedProtocolParamatersAsync(scheme);

                        foreach (var allowedParam in allowedParams)
                        {
                            var item = stored.Raw[allowedParam];
                            if (item != null)
                            {
                                if (string.Compare(allowedParam, "state", true) == 0)
                                {
                                    context.ProtocolMessage.SetParameter(allowedParam, $"{key}.{item}");
                                }
                                else
                                {
                                    context.ProtocolMessage.SetParameter(allowedParam, item);
                                }
                            }
                        }
                        /*
                        if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                        {
                            context.ProtocolMessage.AcrValues = "v1=google";
                        }
                        */

                    };
                    options.Events.OnRemoteFailure = context =>
                    {
                        context.Response.Redirect("/");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    };
                });
            }


            return new IdentityBuilder(typeof(TUser), services);
        }
    }
}
