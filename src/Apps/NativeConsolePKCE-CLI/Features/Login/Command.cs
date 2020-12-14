using AutoMapper;
using Common;
using IdentityModel.OidcClient;
using McMaster.Extensions.CommandLineUtils;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluffyBunny4.DotNetCore;
using FluffyBunny4.DotNetCore.Services;

namespace NativeConsolePKCE_CLI.Features.Login
{
    public static class Commands
    {
        [Command("login", Description = "login")]
        public class LoginCommand
        {
            private ISerializer _serializer;

            [Option("-c|--context-token", CommandOptionType.SingleValue, Description = "The context token")]
            public string ContextToken { get; set; }
            private async Task OnExecuteAsync(
               IConsole console,
               IMediator mediator,
               ISerializer serializer,
               IMapper mapper,
               LoginInfo.Request request)
            {
                _serializer = serializer;
                using (new DisposableStopwatch(t => console.WriteLine($"LoginCommand - {t} elapsed")))
                {
                    Validate(); var command = mapper.Map(this, request);
                    var response = await mediator.Send(command);
                    if (response.Exception != null)
                    {
                        console.WriteLine($"{response.Exception.Message}");
                    }
                    else
                    {
                        ShowResult(response.Result);
                    }
                }
            }
            private void Validate()
            {
                StringBuilder sb = new StringBuilder();
                bool error = false;
                if (string.IsNullOrWhiteSpace(ContextToken))
                {
                    error = true;
                    sb.Append($"--context-token is missing\n");
                }


                if (error)
                {
                    throw new Exception(sb.ToString());
                }
            }
            private void ShowResult(LoginResult result)
            {
                if (result.IsError)
                {
                    Console.WriteLine("\n\nError:\n{0}", result.Error);
                    return;
                }

                Console.WriteLine("\n\nClaims:");
                foreach (var claim in result.User.Claims)
                {
                    Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
                }

                Console.WriteLine($"\nidentity token: {result.IdentityToken}");
                Console.WriteLine($"access token:   {result.AccessToken}");
                Console.WriteLine($"refresh token:  {result?.RefreshToken ?? "none"}");

                var values = _serializer.Deserialize<Dictionary<string, object>>(result.TokenResponse.Raw);

                Console.WriteLine($"");
                foreach (var item in values)
                {
                    Console.WriteLine($"{item.Key}: {item.Value}");
                }
            }
        }
    }
}
