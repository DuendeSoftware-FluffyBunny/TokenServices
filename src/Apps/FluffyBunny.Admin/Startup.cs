using FluffyBunny.Admin.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluffyBunny.Admin.Model;
using FluffyBunny4.DotNetCore;
using FluffyBunny4.DotNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FluffyBunny.Admin
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostEnvironment HostingEnvironment { get; }
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
                var appOptions = Configuration
                    .GetSection("AppOptions")
                    .Get<AppOptions>();
                services.Configure<AppOptions>(Configuration.GetSection("AppOptions"));

                services.AddTransient<ISigninManager, DefaultSigninManager>();
                services.AddScoped<IUserClaimsPrincipalFactory<IdentityUser>, SeedSessionClaimsPrincipalFactory>();

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(
                        Configuration.GetConnectionString("DefaultConnection")));
                services.AddDatabaseDeveloperPageExceptionFilter();

                services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

              

                services.AddAuthentication();


                services.AddControllers();
                IMvcBuilder builder = services.AddRazorPages();
                if (HostingEnvironment.IsDevelopment())
                {
                    builder.AddRazorRuntimeCompilation();
                }

                // Adds a default in-memory implementation of IDistributedCache.
                services.AddDistributedMemoryCache();

                services.AddSession(options =>
                {
                    options.Cookie.IsEssential = true;
                    options.Cookie.Name = $"{Configuration["applicationName"]}.Session";
                    // Set a short timeout for easy testing.
                    options.IdleTimeout = TimeSpan.FromSeconds(appOptions.AuthAndSessionCookies.TTL);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });

            }
            catch (Exception ex)
            {
                _deferedException = ex;
            }
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
                app.UseDatabaseErrorPage();
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

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<AuthSessionValidationMiddleware>();
     //      app.UseMiddleware<EnsureSessionTenantMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
