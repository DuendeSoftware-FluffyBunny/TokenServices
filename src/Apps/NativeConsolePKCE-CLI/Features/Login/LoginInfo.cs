using Common;
using IdentityModel.OidcClient;
using MediatR;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluffyBunny4.DotNetCore.Services;

namespace NativeConsolePKCE_CLI.Features.Login
{
    public static class LoginInfo
    {
        public class Request : IRequest<Response>
        {
            public ISerializer Serializer { get; }
            public string ContextToken { get; set; }

            public Request(ISerializer serializer)
            {
                Serializer = serializer;
            }
        }

        public class Response
        {
            public Exception Exception { get; set; }
            public LoginResult Result { get; internal set; }
        }
        public class Handler : IRequestHandler<Request, Response>
        {
            private const string ServiceName = "myphotos";
            private const string ScopeBaseUrl = "https://www.companyapis.com/auth";

            static string _clientId = "1096301616546-edbl612881t7rkpljp3qa3juminskulo.apps.googleusercontent.com";
            static string _authority = "https://localhost:6601";

            public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                Response response = new Response { };
                try
                {
                    var browser = new SystemBrowser(45656);
                    string redirectUri = "http://127.0.0.1:45656";

                    var scopes = new[] {
                        $"openid",
                        $"profile",
                        $"{ScopeBaseUrl}/{ServiceName}",
                        $"{ScopeBaseUrl}/{ServiceName}.readonly",
                        $"{ScopeBaseUrl}/{ServiceName}.modify",
  
                    };

                    var scope = string.Join(" ", scopes);
                    var options = new OidcClientOptions
                    {
                        Authority = _authority,
                        ClientId = _clientId,
                        RedirectUri = redirectUri,
                        Scope = scope, //"openid profile"
                        FilterClaims = false,
                        Browser = browser,
                        Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                        ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                        LoadProfile = true
                        

                    };
                    options.Policy.Discovery.ValidateIssuerName = false;
                    options.Policy.Discovery.ValidateEndpoints = false;
                    var serilog = new LoggerConfiguration()
                       .MinimumLevel.Verbose()
                       .Enrich.FromLogContext()
                       .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}")
                       .CreateLogger();

                    options.LoggerFactory.AddSerilog(serilog);

                    var oidcClient = new OidcClient(options);

                    Dictionary<string, string> extra = new Dictionary<string, string>
                    {
                        ["context-token"] = request.ContextToken
                    };

                    //   ResponseValidationResult rvr = null;
                    response.Result = await oidcClient.LoginAsync(new LoginRequest()
                    {
                        FrontChannelExtraParameters = extra,
                       
                        
                    });

                }
                catch (Exception ex)
                {
                    response.Exception = ex;
                }

                return response;
            }
        }
    }
}
