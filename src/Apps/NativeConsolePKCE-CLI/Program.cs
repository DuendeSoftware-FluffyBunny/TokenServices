using AutoMapper;
using Common;
using McMaster.Extensions.CommandLineUtils;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NativeConsolePKCE_CLI.Features.Login;
using System;
using System.Reflection;
using System.Threading.Tasks;
using static NativeConsolePKCE_CLI.Features.Login.Commands;

namespace NativeConsolePKCE_CLI
{
    [Command(
      Name = "NativeConsolePKCE_CLI",
      Description = @"NativeConsolePKCE_CLI
    ")]
    [HelpOption]
    [VersionOptionFromMember(MemberName = "GetVersion")]
    [Subcommand(
      typeof(LoginCommand)

      )
    ]
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {

                await CreateHostBuilder()
                    .RunCommandLineApplicationAsync<Program>(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddMediatR(typeof(Program).Assembly)
                        .AddAutoMapper(typeof(Program).Assembly);
                    services.AddSingleton<ISerializer, Serializer>();


                    services.AddTransient<LoginInfo.Request>();

                });
        }
        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("You must specify a command");
            app.ShowHelp();
            return 1;
        }

        private string GetVersion()
        {
            return typeof(Program).Assembly
                ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
        }
    }
}
