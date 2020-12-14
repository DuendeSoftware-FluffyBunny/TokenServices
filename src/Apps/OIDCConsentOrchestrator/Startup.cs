using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OIDCConsentOrchestrator.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using OIDCConsentOrchestrator.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIdConnectModels;
using Nito.AsyncEx;
using OIDCConsentOrchestrator.Models.Client;
using System.Net.Http;
using System.Text.Json;
using FluffyBunny4.DotNetCore;
using OIDCConsentOrchestrator.Services;
using OIDCPipeline.Core;
using OIDCConsentOrchestrator.Authentication;
using OIDCPipeline.Core.Extensions;
using OIDCPipeline.Core.Services;
using IdentityModel.FluffyBunny4.Extensions;
using FluffyBunny4.DotNetCore.Services;
using OIDCConsentOrchestrator.Extensions;
using FluffyBunny4.DotNetCore.Identity;
using FluffyBunny4.DotNetCore.Services.Defaults;

namespace OIDCConsentOrchestrator
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostEnvironment HostingEnvironment { get; }
        public List<OpenIdConnectSchemeRecord> OpenIdConnectSchemeRecords { get; set; }
        public Dictionary<string, OIDCSchemeRecord> OIDCOptionStore { get; set; }
        public AppOptions AppOptions { get; private set; }

        private ILogger _logger;
        private Exception _deferedException;
        public Startup(
            IConfiguration configuration,
            IHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
            _logger = new LoggerBuffered(LogLevel.Debug);
            _logger.LogInformation($"Create Startup {hostingEnvironment.ApplicationName} - {hostingEnvironment.EnvironmentName}");
        }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {
                AppOptions = Configuration
                      .GetSection("AppOptions")
                      .Get<AppOptions>();
                _logger.LogDebug("AppOptions");
                _logger.LogDebug(ToJson(AppOptions));

                services.Configure<DataProtectionOptions>(Configuration.GetSection("DataProtectionOptions"));
                services.Configure<FluffyBunny4TokenServiceConfiguration>(Configuration.GetSection("FluffyBunny4TokenServiceConfiguration"));


                var fbtsc = Configuration
                    .GetSection("FluffyBunny4TokenServiceConfiguration")
                    .Get<FluffyBunny4TokenServiceConfiguration>();
                _logger.LogDebug("FluffyBunny4TokenServiceConfiguration");
                _logger.LogDebug(ToJson(fbtsc));

                OpenIdConnectSchemeRecords = Configuration
                  .GetSection("OpenIdConnect")
                  .Get<List<OpenIdConnectSchemeRecord>>();

                OIDCOptionStore = Configuration
                    .GetSection("oidcOptionStore")
                    .Get<Dictionary<string, OIDCSchemeRecord >>();

              
                services.AddSingleton<IOIDCPipelineClientStore>(sp =>
                {
                    return new InMemoryClientSecretStore(OIDCOptionStore);
                });

                var openIdConnectSchemeRecordSchemeRecords = Configuration
                    .GetSection("openIdConnect")
                    .Get<List<OpenIdConnectSchemeRecord>>();


                services.AddHttpClient();

                Func<HttpMessageHandler> configureHandler = () =>
                {
                    var handler = new HttpClientHandler
                    {
                        //!DO NOT DO IT IN PRODUCTION!! GO AND CREATE VALID CERTIFICATE!
                       
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                    return handler;
                };

                if (AppOptions.DangerousAcceptAnyServerCertificateValidator)
                {
                    services.AddHttpClient("HttpClient").ConfigurePrimaryHttpMessageHandler(configureHandler);
                    services.AddHttpClient("token-service").ConfigurePrimaryHttpMessageHandler(configureHandler);
                }
                else
                {
                    services.AddHttpClient("HttpClient");
                    services.AddHttpClient("token-service");
                }



                services.AddSingleton<IOpenIdConnectSchemeRecords>(new InMemoryOpenIdConnectSchemeRecords(openIdConnectSchemeRecordSchemeRecords));

                services.AddDistributedMemoryCache();
                services.AddAuthentication();
                services.AddAuthentication<IdentityUser>(OpenIdConnectSchemeRecords);
                services.AddGoogleDiscoveryCache();
                services.AddTokenServiceDiscoveryCache();

                //    services.AddAuthentication<IdentityUser>(Configuration);
                services.AddScoped<ISigninManager, DefaultSigninManager>();
                services.AddDbContext<ApplicationDbContext>(config =>
                {
                    // for in memory database  
                    config.UseInMemoryDatabase("InMemoryDataBase");
                });
                services.AddScoped<IEmailSender, NullEmailSender>();

                services.AddDatabaseDeveloperPageExceptionFilter();
                services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();
                /*
                 * services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                      .AddEntityFrameworkStores<ApplicationDbContext>();
                */

                services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, SeedSessionClaimsPrincipalFactory>();

                services.AddOIDCPipeline(options =>
                {
                    options.Scheme = AppOptions.DownstreamAuthorityScheme; // Google
                    options.PostAuthorizeHookRedirectUrl = $"/Identity/Account/ExternalLogin?handler=Provider&provider={AppOptions.DownstreamAuthorityScheme}&returnUrl=/AuthorizeConsent";
                });
                services.AddDistributedCacheOIDCPipelineStore(options =>
                {
                    options.ExpirationMinutes = 30;
                });
              

                services.AddControllers()
                                   .AddSessionStateTempDataProvider();
                IMvcBuilder builder = services.AddRazorPages();
                builder.AddSessionStateTempDataProvider();
                if (HostingEnvironment.IsDevelopment())
                {
                    builder.AddRazorRuntimeCompilation();
                }

                #region COOKIES
                //*************************************************
                //*********** COOKIE Start ************************
                //*************************************************


                services.Configure<CookiePolicyOptions>(options =>
                {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });
                services.ConfigureExternalCookie(config =>
                {
                    config.Cookie.SameSite = SameSiteMode.None;
                });
                services.ConfigureApplicationCookie(options =>
                {
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                    options.ExpireTimeSpan = TimeSpan.FromSeconds(AppOptions.CookieTTL);
                    options.SlidingExpiration = true;
                 //   options.Cookie.Name = $"{Configuration["applicationName"]}.AspNetCore.Identity.Application";
                    options.LoginPath = $"/Identity/Account/Login";
                    options.LogoutPath = $"/Identity/Account/Logout";
                    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
                    options.Events = new CookieAuthenticationEvents()
                    {

                        OnRedirectToLogin = (ctx) =>
                        {
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == StatusCodes.Status200OK)
                            {
                                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return Task.CompletedTask;
                            }
                            ctx.Response.Redirect(ctx.RedirectUri);
                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = (ctx) =>
                        {
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == StatusCodes.Status200OK)
                            {
                                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                                return Task.CompletedTask;
                            }
                            ctx.Response.Redirect(ctx.RedirectUri);
                            return Task.CompletedTask;
                        }
                    };
                });


                //*************************************************
                //*********** COOKIE END **************************
                //*************************************************
                #endregion
                #region SESSION
                services.AddSession(options =>
                {
                    // options.Cookie.Name = $"{Configuration["applicationName"]}.Session";
                    options.Cookie.IsEssential = true;
                    // Set a short timeout for easy testing.
                    options.IdleTimeout = TimeSpan.FromSeconds(AppOptions.CookieTTL);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });
                #endregion

                services.AddFluffyBunnyTokenService();
                services.AddSingleton<IBinarySerializer, BinarySerializer>();
                services.AddSerializers();
            }
            catch (Exception ex)
            {
                _deferedException = ex;
            }
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
           IApplicationBuilder app,
           IWebHostEnvironment env,
           IServiceProvider serviceProvider,
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
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseOIDCPipeline();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<AuthSessionValidationMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
