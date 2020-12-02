using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.Stores;
using Microsoft.EntityFrameworkCore;
using TestWebApp;
using FluentAssertions;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Entities;

namespace XUnitTest_EntityFramework
{


    // https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-3.1
    // https://wellsb.com/csharp/aspnet/xunit-unit-test-razor-pages/

    public class UnitTest_EntityFramework :
        IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private CustomWebApplicationFactory<Startup> _factory;
        private HttpClient _client;

        public UnitTest_EntityFramework(
            CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
                });
            }).CreateClient();
        }
        [Fact]
        public async Task Ensure_ITenantAwareDbContextAccessor_Async()
        {
            var serviceProvider = _factory.Server.Services;
            using (var scope = serviceProvider.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var tenantAwareDbContextAccessor = sp.GetRequiredService<ITenantAwareConfigurationDbContextAccessor>();
                tenantAwareDbContextAccessor.Should().NotBeNull();
            }
        }
      
         
        [Fact]
        public async Task Ensure_ITenantAwareDbContextAccessor_create_client_Async()
        {
            var serviceProvider = _factory.Server.Services;
            using (var scope = serviceProvider.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var tenantAwareDbContextAccessor = sp.GetRequiredService<ITenantAwareConfigurationDbContextAccessor>();
                tenantAwareDbContextAccessor.Should().NotBeNull();
                var tenantContext = tenantAwareDbContextAccessor.GetTenantAwareConfigurationDbContext("test2");
                tenantContext.Should().NotBeNull();

                await tenantContext.Clients.AddAsync(new ClientExtra()
                {
                    ClientId = "test-client"
                });
                await tenantContext.SaveChangesAsync();
                var c = await tenantContext.Clients.FirstAsync(c => c.ClientId == "test-client");
                c.Should().NotBeNull();
                tenantContext.Clients.Remove(c);
                await tenantContext.SaveChangesAsync();
                c = await tenantContext.Clients.FirstOrDefaultAsync(c => c.ClientId == "test-client");
                c.Should().BeNull();
            }
        }
        [Fact]
        public async Task Ensure_IMainEntityCoreContext_Async()
        {
            var serviceProvider = _factory.Server.Services;
            using (var scope = serviceProvider.CreateScope())
            {
                var sp = scope.ServiceProvider;
                var mainEntityCoreContext = sp.GetRequiredService<IMainEntityCoreContext>();
                mainEntityCoreContext.Should().NotBeNull(); 
          
                var tenantEntity = new Tenant()
                {
                    Name = "bob",
                    Enabled = true
                }; 
                var result = await mainEntityCoreContext.Tenants.AddAsync(tenantEntity);
                await mainEntityCoreContext.SaveChangesAsync();
                var tenantInDb = await mainEntityCoreContext.Tenants.FirstOrDefaultAsync(
                    t => t.Name == tenantEntity.Name);
                tenantInDb.Should().NotBeNull();
                tenantInDb.Name.Should().Be(tenantEntity.Name);
            }
        }
    }
}
