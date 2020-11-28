using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Options;
using FluffyBunny.IdentityServer.EntityFramework.Storage.DbContexts;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Extensions;
using MigrationHostApp.Models;
using Microsoft.Extensions.Configuration;

namespace MigrationHostApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostEnvironment HostingEnvironment { get; }

        public Startup(
            IConfiguration configuration,
            IHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var appOptions = Configuration
                .GetSection("AppOptions")
                .Get<AppOptions>();
            services.Configure<EntityFrameworkConnectionOptions>(
                Configuration.GetSection("EntityFrameworkConnectionOptions"));
            switch (appOptions.DatabaseType)
            {
                case AppOptions.DatabaseTypes.SqlServer:
                    services.AddSqlServerDbContextOptionsProvider();
                    services.AddSingleton<IMigrationsAssemblyProvider, SqlServerMigrationsAssemblyProvider>();
                    break;
            }
            services.AddDbContextTenantServices();
            var options = new ConfigurationStoreOptions();
            services.AddSingleton(options);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
