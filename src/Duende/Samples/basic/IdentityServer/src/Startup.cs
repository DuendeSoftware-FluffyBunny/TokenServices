// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Duende.IdentityServer;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServerHost
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            var password = "1234";
            var ecdsaPFX = @"MIID9wIBAzCCA7MGCSqGSIb3DQEHAaCCA6QEggOgMIIDnDCCAW0GCSqGSIb3DQEHAaCCAV4EggFaMIIBVjCCAVIGCyqGSIb3DQEMCgECoIHMMIHJMBwGCiqGSIb3DQEMAQMwDgQI6Rn/G4nB1SACAgfQBIGoRg/w/KUDAumcM0STBZ
DYefbKsk3M6p7qILoOIfD2k4i9gkL0+JKWbSSPzOoAtKE9FhGbT2WC1Vyt1CjRsRkPqtCpkU/D2rVwq9BKX483+1x+2JqhuhkMU1NAejn5QUqCPmjKPnWfjVXRPGMmYQoIKK+B9uZiPk0k3FlLC/6/JPI51wA1rwGuor99vgaU02fauNcRDv/CCpWCAeSUZAZApY6jRE9Wwi93MXQwEwYJKoZIhvcNAQkVMQYEBAEAAAAwXQYJKwYBBAGCNxEBMVAeTgBNAGkAYwByAG8AcwBvAGYAdAAgAFMAbwBmAHQAdwBhAHIAZQAgAEsAZQB5ACAAUwB0AG8AcgBhAGcAZQAgAFAAcgBvAHYAaQBkAGUAcjCCAicGCSqGSIb3DQEHBqCCAhgwggIUAgEAMIICDQYJKoZIhvcNAQcBMBwGCiqGSIb3DQEMAQMwDgQI6jHZeCkfSu4CAgfQgIIB4Cx+XJ79QcELNweAZH/LWrTR25Im0HJ/SfATqnNcOvfluz3TxuJOrJUy15exisVjKSBynD6Tl4IyIwLSfJoC9N3ydaMcOvcKATdGDEd5nX8zSWNaJGCJXMQAU3M/YHo+vrOOjatOEs65/3qQSrEITcEPVqN6/lfoBPpZh10NIiGuzhXqbSulki45f2Tly43ondBosqX3NzSQzOlAKmgivMeGxL+mMfpe9IkgHfIsJ9uu4x6iaSKiHdeAzHLtINKQzianHEmiusOaz9hrE0F5AO3NUM8nxtLFouQwX0JreXeFsZPriIqZ4b9p6ulEnw6MC0TjZZ9OOj7MGcvvHQeXCufnQL3FpUghzzG7lBx50Ovf9KzL7lG+mLIU8fkJ19bx6izYwVSHZa0CtZg28bF/mqC19Qi6ev/9j0nC9MmsfA25fnKpe7Ob2ddbkXSa8v6whVTY5C/digfrtKeoQay3B1gTZXKdiRH700ejS6FZ1EliMfMfRYczyc6kYsrBez0AlvkaozgqX8wyD4Z1iSjwYSLmOlguvCIuhDV3crltkjODGRHlyckdlWEJNH0PIJBr9qG1jRnA1t/MWdJK9gHTX2VqwHtPZKxikQq6x1h/hFMpU8ZahD1y886JLnBiBqxkIzA7MB8wBwYFKw4DAhoEFMO4ZPmUTPmNn7Xn57uXiyIWyttZBBTlMlP5OzM3CMlqmNdJOPK+iiOEwQICB9A=";

            byte[] ecdsaCertPfxBytes = Convert.FromBase64String(ecdsaPFX);
            var ecdsaCertificate = new X509Certificate2(ecdsaCertPfxBytes, password);

         

            ECDsaSecurityKey ecdsaCertificatePublicKey = new ECDsaSecurityKey(ecdsaCertificate.GetECDsaPrivateKey());


            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v5/basics/resources
                options.EmitStaticAudienceClaim = true;
                options.KeyManagement.Enabled = false;
            })
                .AddTestUsers(TestUsers.Users);
            builder.AddInMemoryIdentityResources(Resources.Identity);
            builder.AddInMemoryApiScopes(Resources.ApiScopes);
            builder.AddInMemoryApiResources(Resources.ApiResources);
            builder.AddInMemoryClients(Clients.List);
            
            // this is only needed for the JAR and JWT samples and adds supports for JWT-based client authentication
            builder.AddJwtBearerClientAuthentication();

            services.AddAuthentication()
                .AddOpenIdConnect("Google", "Sign-in with Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ForwardSignOut = IdentityServerConstants.DefaultCookieAuthenticationScheme;

                    options.Authority = "https://accounts.google.com/";
                    options.ClientId = "708778530804-rhu8gc4kged3he14tbmonhmhe7a43hlp.apps.googleusercontent.com";

                    options.CallbackPath = "/signin-google";
                    options.Scope.Add("email");
                });
            builder.AddSigningCredential(ecdsaCertificatePublicKey, IdentityServerConstants.ECDsaSigningAlgorithm.ES256);

        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}