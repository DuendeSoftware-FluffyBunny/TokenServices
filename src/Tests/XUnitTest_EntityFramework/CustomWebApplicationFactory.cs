using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace XUnitTest_EntityFramework
{
    /*
     public class SessionTenantAccessor : ISessionTenantAccessor
    {
        public SessionTenantAccessor()
        {

        }
        public string TenantId { get; set; }
    } 

     */
    public class CustomWebApplicationFactory<TStartup> :
        WebApplicationFactory<TStartup>
        where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            //            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                //   services.AddScoped<ISessionTenantAccessor, SessionTenantAccessor>();
            });
        }
    }
}