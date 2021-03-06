using FluffyBunny4.DotNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CorrelationId.DependencyInjection;
using CorrelationId.HttpClient;
using Duende.IdentityServer;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.EntityFramework.Options;
using Duende.IdentityServer.ResponseHandling;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using FluffyBunny.EntityFramework.Context;
using FluffyBunny.EntityFramework.Context.Extensions;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Extensions;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Models;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Stores;
using Microsoft.AspNetCore.Http;
using TokenService.Models;
using FluffyBunny4.Configuration;
using FluffyBunny4.Services;
using FluffyBunny4.Services.Default;
using FluffyBunny4.Validation;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.HttpOverrides;
using FluffyBunny4.Azure.Configuration.CosmosDB;
using FluffyBunny4.ResponseHandling;
using FluffyBunny4.Stores;

namespace TokenService
{
    public class Startup
    {
        
           

        public IConfiguration Configuration { get; }
        public IHostEnvironment HostingEnvironment { get; }
        public AppOptions AppOptions { get; private set; }

        private ILogger _logger;
        private Exception _deferedException;
        public Startup(
            IConfiguration configuration,
            IHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
            if (Global.LoggerProvider != null)
            {
                _logger = Global.LoggerProvider.CreateLogger("b545a46a-99a7-4548-85a7-ba69c4f99279");
            }
            else
            {
                _logger = new LoggerBuffered(LogLevel.Debug);
            }
            if (Global.HostContext == null)
            {
                Global.HostContext = new HostContext()
                {
                    ContentRootPath = HostingEnvironment.ContentRootPath
                };

            }

            _logger.LogInformation($"Create Startup {hostingEnvironment.ApplicationName} - {hostingEnvironment.EnvironmentName}");
        }

        string ToJson<T>(T obj)
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(obj, jsonSerializerOptions);
        }

        void LogOptions<T>(string key) where T:class
        {
            var config = Configuration
                .GetSection(key)
                .Get<T>();
            _logger.LogDebug($"{key}:{ToJson(config)}");
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            try
            {
                services.AddSerializers();
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                AppOptions = Configuration
                    .GetSection("AppOptions")
                    .Get<AppOptions>();
                _logger.LogDebug($"appOptions:{ToJson(AppOptions)}");
                services.Configure<AppOptions>(Configuration.GetSection("AppOptions"));
                var keyVaultSigningOptions = Configuration
                    .GetSection("KeyVaultSigningOptions")
                    .Get<KeyVaultSigningOptions>();
                

                LogOptions<KeyVaultSigningOptions>("KeyVaultSigningOptions");
                LogOptions<CosmosDbConfiguration>("CosmosDbConfiguration");
                LogOptions<IdentityServerOptions>("IdentityServer");
                LogOptions<TokenExchangeOptions>("TokenExchangeOptions");
                LogOptions<ExternalServicesOptions>("ExternalServicesOptions");

                services.Configure<IdentityServerOptions>(Configuration.GetSection("IdentityServer"));
                services.Configure<KeyVaultSigningOptions>(Configuration.GetSection("KeyVaultSigningOptions"));
                services.Configure<TokenExchangeOptions>(Configuration.GetSection("TokenExchangeOptions"));
                services.Configure<ExternalServicesOptions>(Configuration.GetSection("ExternalServicesOptions"));
                services.Configure<SelfManagedCertificatesOptions>(Configuration.GetSection("SelfManagedCertificatesOptions"));
                var dd = Configuration
                    .GetSection("SelfManagedCertificatesOptions")
                    .Get<SelfManagedCertificatesOptions>();

                _logger.LogInformation($"HostingEnvironment.EnvironmentNam:{HostingEnvironment.EnvironmentName}");
                if (AppOptions.DangerousAcceptAnyServerCertificateValidator)
                {
                    Func<HttpMessageHandler> configureHandler = () =>
                    {

                        var handler = new HttpClientHandler
                        {
                            //!DO NOT DO IT IN PRODUCTION!! GO AND CREATE VALID CERTIFICATE!

                            ServerCertificateCustomValidationCallback =
                                (httpRequestMessage, x509Certificate2, x509Chain, sslPolicyErrors) => { return true; }
                        };

                        return handler;
                    };


                    services.AddHttpClient(FluffyBunny4.Constants.ExternalServiceClient.HttpClientName)
                        .ConfigurePrimaryHttpMessageHandler(configureHandler);
                }
                else
                {
                    services.AddHttpClient(FluffyBunny4.Constants.ExternalServiceClient.HttpClientName)
                        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler());
                }

                // set forward header keys to be the same value as request's header keys
                // so that redirect URIs and other security policies work correctly.
                var aspNETCORE_FORWARDEDHEADERS_ENABLED = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_FORWARDEDHEADERS_ENABLED"), "true", StringComparison.OrdinalIgnoreCase);
                if (aspNETCORE_FORWARDEDHEADERS_ENABLED)
                {
                    //To forward the scheme from the proxy in non-IIS scenarios
                    services.Configure<ForwardedHeadersOptions>(options =>
                    {
                        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                        // Only loopback proxies are allowed by default.
                        // Clear that restriction because forwarders are enabled by explicit 
                        // configuration.
                        options.KnownNetworks.Clear();
                        options.KnownProxies.Clear();
                    });
                }
                _logger.LogInformation($"ASPNETCORE_FORWARDEDHEADERS_ENABLED = ${aspNETCORE_FORWARDEDHEADERS_ENABLED}");

                services.AddHttpClient("MyClient")
                    .AddCorrelationIdForwarding(); // add the handler to attach the correlation ID to outgoing requests for this named client
                services.AddDefaultCorrelationId(options => {
                    options.AddToLoggingScope = true;
                });
                services.AddHostStorage();
                services.AddScopedStorage();
                services.AddScopedServices();
                
                //////////////////////////////////////////////
                // refresh_token grace feature begin
                //////////////////////////////////////////////
                services.AddGraceRefreshTokenService();

                services.AddFluffyBunny4AutoMapper();

                //////////////////////////////////////////////
                // refresh_token grace feature end
                //////////////////////////////////////////////


                ///////////////////////////////////////////////////////////////////////////////
                /// AddCosmosOperationalStore
                ///////////////////////////////////////////////////////////////////////////////
                _logger.LogInformation("ConfigureServices - AddCosmosOperationalStore ");
                /*
                var cosmosPrimaryKeyVaultFetchStore = new SimpleStringKeyVaultFetchStore(
                    new KeyVaultFetchStoreOptions<string>()
                    {
                        ExpirationSeconds = 3600,
                        KeyVaultName = keyVaultName,
                        SecretName = cosmosPrimaryKeySecretName
                    }, ManagedIdentityHelper.CreateKeyVaultClient(_logger), _logger);
                var primaryKey = cosmosPrimaryKeyVaultFetchStore.GetStringValueAsync().GetAwaiter().GetResult();
                */
                services.Configure<CosmosDbConfiguration>(Configuration.GetSection("CosmosDbConfiguration"));


                services.AddTransient<IResourceValidator, MyDefaultResourceValidator>();
                services.AddClaimsService<MyDefaultClaimsService>();

                var jsonClientFile = Path.Combine(
                    Global.HostContext.ContentRootPath, "settings/clients.json");
                var jsonApiScopesFile = Path.Combine(
                    Global.HostContext.ContentRootPath, "settings/api-scopes.json");
                var jsonApiResourcesFile = Path.Combine(
                    Global.HostContext.ContentRootPath, "settings/api-resources.json");

                var options = new ConfigurationStoreOptions();
                services.AddSingleton(options);

                var operationalStoreOptions = new OperationalStoreOptions
                {
                    EnableTokenCleanup = true
                };
                services.AddSingleton(operationalStoreOptions);

                services.AddSingleton<IConsentExternalService, ConsentExternalService>();

                _logger.LogInformation("ConfigureServices - AddIdentityServer ");

                services.AddSingleton<IHashFixer, HashFixer>();
                services.AddTransient<ITokenResponseGenerator, MyTokenResponseGenerator>();
                var builder = services.AddIdentityServer()
                    //.AddInMemoryIdentityResources(Config.IdentityResources)
                    //     .AddInMemoryApiScopes(Config.ApiScopes)
                    //     .AddInMemoryClientExtras(jsonClientFile)
                    .AddEntityFrameworkStores()
                    .AddInMemoryCaching()
                    .AddExtensionGrantValidator<ArbitraryIdentityGrantValidator>()
                    .AddExtensionGrantValidator<ArbitraryTokenGrantValidator>()
                    .AddExtensionGrantValidator<TokenExchangeGrantValidator>()
                    .AddExtensionGrantValidator<TokenExchangeMutateGrantValidator>()
                    .SwapOutRefreshTokenStore()
                    .SwapOutReferenceTokenStore()
                    .SwapOutTokenRevocationRequestValidator()
                    .SwapOutTokenRevocationResponseGenerator()
                    .SwapOutTokenService<MyDefaultTokenService>();

                // services.AddTenantAdminServices();
                var entityFrameworkConnectionOptions = Configuration
                    .GetSection("EntityFrameworkConnectionOptions")
                    .Get<EntityFrameworkConnectionOptions>();
                services.Configure<EntityFrameworkConnectionOptions>(Configuration.GetSection("EntityFrameworkConnectionOptions"));
                if (HostingEnvironment.IsDevelopment())
                {
                    _logger.LogDebug(
                        $"entityFrameworkConnectionOptions:{ToJson(entityFrameworkConnectionOptions)}");
                }

                switch (AppOptions.DatabaseType)
                {
                    default:
                    case AppOptions.DatabaseTypes.InMemory:
                        services.AddInMemoryDbContextOptionsProvider();
                        break;
                    case AppOptions.DatabaseTypes.Postgres:
                        //     services.AddEntityFrameworkNpgsql();
                        services.AddPostgresDbContextOptionsProvider();
                        break;
                    case AppOptions.DatabaseTypes.CosmosDB:
                        //    services.AddEntityFrameworkCosmos();
                        services.AddCosmosDbContextOptionsProvider();
                        break;
                    case AppOptions.DatabaseTypes.SqlServer:
                        //    services.AddEntityFrameworkCosmos();
                        services.AddSqlServerDbContextOptionsProvider();
                        break;
                }



                services.AddDbContextTenantServices();

                //  builder.AddDeveloperSigningCredential();



                //////////////////////////////////////////////
                // IdentityServer sometimes doesn't do a TryAddTransient
                // so we have to replace the services with a remove then add.
                // I like a concept of only one service in a system that only is designed for that one service.
                // IServiceCollection lets you add multiple times and the way you get all of them is to ask for
                // IEnumerable<IClientSecretValidator>.  If you just want one, you ask for IClientSecretValidator and you get 
                // the last one added.  Hence, I like replace == remove + add.
                //////////////////////////////////////////////
                // replace IdentityServer's IClientSecretValidator with mine.
                // note: This isn't needed for the refesh_token grace stuff
                //       This is to allow a refresh_token to be redeemed without a client_secret
                services.SwapOutClientSecretValidator<FluffyBunnyClientSecretValidator>();
                services.SwapOutIntrospectionResponseGenerator<FluffyBunnyIntrospectionResponseGenerator>();
                services.SwapOutCustomTokenRequestValidator<FluffyBunnyCustomTokenRequestValidator>();
                services.SwapOutDeviceCodeValidator<MyDeviceCodeValidator>();
                


                // BASICALLY to make sure your stuff is the one being used, add it last.

                ///////////////////////////////////////////////////////////////////////////////
                // TenantServices
                ///////////////////////////////////////////////////////////////////////////////
                // if you add this it locks you into an url that is {identityServerBase}/{tenantId}/{endpoints} vs
                // {identityServerBase}/{endpoints}
                // I utilize IScopedContext<TenantRequestContext> a SCOPED object that you can inject anywhere in the pipeline to tell you what tenant we have.
                ///////////////////////////////////////////////////////////////////////////////
                _logger.LogInformation("ConfigureServices - AddTenantServices ");
                services.AddTenantServices();
                services.AddTenantResolverCache<TenantResolver>();


                if (keyVaultSigningOptions.Enabled)
                {
                    ///////////////////////////////////////////////////////////////////////////////
                    /// Add KeyVaultSigningServices
                    ///////////////////////////////////////////////////////////////////////////////
                    _logger.LogInformation("ConfigureServices - KeyVaultSigningServices ");
                    services.AddKeyVaultTokenCreationServices();

                    switch (keyVaultSigningOptions.SigningType)
                    {
                        case KeyVaultSigningOptions.SigningTypes.KeyVaultCertificate:
                        {
                            services.AddKeyVaultCertificatesStores(options =>
                            {
                                options.ExpirationSeconds = 43200;
                                options.KeyVaultName = keyVaultSigningOptions.KeyVaultName;
                                options.KeyIdentifier = "fluffy-oauth2";
                            });
                            services.AddAzureKeyVaultCertificateSignatureProvider();
                        }
                            break;
                        case KeyVaultSigningOptions.SigningTypes.KeyVaultECDsaKey:
                        {

                            services.AddKeyVaultECDsaStores(options =>
                            {
                                options.ExpirationSeconds = 43200;
                                options.KeyVaultName = keyVaultSigningOptions.KeyVaultName;
                                options.KeyIdentifier = "{0}-key-ecc-signing";
                            });

                            services.AddAzureKeyVaultECDsaSignatureProvider();
                        }
                            break;
                        default:
                            throw new Exception("Need a SigningType!");
                    }
                }
                else
                {
                    services.AddSelfManagedValidationKeysStores();
                }
               

                switch (AppOptions.OperationalStoreType)
                {
                    default:
                    case AppOptions.DatabaseTypes.InMemory:
                        services.AddInMemoryTenantAwarePersistedGrantStoreOperationalStore();
                        break;
                    case AppOptions.DatabaseTypes.CosmosDB:
                        services.AddCosmosOperationalStore();
                        break;
                    case AppOptions.DatabaseTypes.EntityFramework:
                        services.AddEntityFrameworkOperationalStore();
                        break;
                }

                services.AddConsentDiscoveryCacheAccessor();
                services.AddDiscoveryCacheAccessor();

                services.AddControllers();
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo {Title = "TokenService", Version = "v1"});
                });
 
            }
            catch (Exception ex)
            {
                _deferedException = ex;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            ILogger<Startup> logger)
        {
            if (_logger is LoggerBuffered)
            {
                (_logger as LoggerBuffered).CopyToLogger(logger);
            }
            _logger = logger;
            _logger.LogInformation("Configure");
            if (_deferedException != null)
            {
                _logger.LogError(_deferedException.Message);
                throw _deferedException;
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TokenService v1"));
            }

            if (!AppOptions.DisableHttpRedirect)
            {
                app.UseHttpsRedirection();
            }
            app.UseForwardedHeaders();

            app.UseRouting();
            app.UseTenantServices();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
