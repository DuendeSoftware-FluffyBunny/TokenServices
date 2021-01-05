using System.Text.Json;
using System.Threading.Tasks;
using FluffyBunny4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SampleExternalService.Controllers
{
    [Route("{tenant}/api/oauth2-authority")]
    [ApiController]
    public class AbstractAuthorityController : ControllerBase
    {
        private IDiscoveryCacheAccessor _discoveryCacheAccessor;
        private ILogger<AbstractAuthorityController> _logger;

        public AbstractAuthorityController(IDiscoveryCacheAccessor discoveryCacheAccessor,
            ILogger<AbstractAuthorityController> logger)
        {
            _discoveryCacheAccessor = discoveryCacheAccessor;
            _logger = logger;
        }
        [HttpGet(".well-known/openid-configuration")]
        public async Task<Duende.IdentityServer.Models.DiscoveryDocument> GetDiscoveryDocumentAsync(string tenant)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var discoveryCache = _discoveryCacheAccessor.GetCache("google");
            var doco = await discoveryCache.GetAsync();
            var disco = JsonSerializer.Deserialize<Duende.IdentityServer.Models.DiscoveryDocument>(doco.Raw, options);
            disco.issuer = $"https://accounts.{tenant}.com";
            return disco;
        }
    }
}