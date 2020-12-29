using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Options;
using FluffyBunny.EntityFramework.Context;
using FluffyBunny.EntityFramework.Context.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
 

namespace MigrateHost
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var entityFrameworkConnectionOptions = Configuration
                .GetSection("EntityFrameworkConnectionOptions")
                .Get<EntityFrameworkConnectionOptions>();
            services.Configure<EntityFrameworkConnectionOptions>(Configuration.GetSection("EntityFrameworkConnectionOptions"));
            services.AddSingleton<IMigrationsAssemblyProvider, SqlServerMigrationsAssemblyProvider>();
            services.AddSqlServerDbContextOptionsProvider();
            services.AddDbContext<TenantAwareConfigurationDbContext>((serviceProvider, optionsBuilder) => {
                // for NON-INMEMORY  - TenantAwareConfigurationDbContext
                // this is only here so that migration models can be created.
                // we then use it as a template to not only create the new database for the tenant, but
                // downstream using it as a normal connection.

                var dbContextOptionsProvider = serviceProvider.GetRequiredService<IDbContextOptionsProvider>();
                dbContextOptionsProvider.Configure(optionsBuilder);
            });
            services.AddDbContext<MainEntityCoreContext>((serviceProvider, optionsBuilder) => {
                var dbContextOptionsProvider = serviceProvider.GetRequiredService<IDbContextOptionsProvider>();
                dbContextOptionsProvider.Configure(optionsBuilder);
            });
            services.AddSingleton(new ConfigurationStoreOptions());
            services.AddSingleton(new OperationalStoreOptions
            {
                EnableTokenCleanup = true
            });
       
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
