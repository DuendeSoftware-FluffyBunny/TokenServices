using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluffyBunny4;
using FluffyBunny4.Models;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Options;
using SampleExternalService.Models;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SampleExternalService.Controllers
{
    [Route("{tenant}/api/[controller]")]
    [ApiController]
    public class ConsentController : ControllerBase
    {
        private static string GuidS => Guid.NewGuid().ToString();
        public class MyCustom
        {
            public class Inner
            {
                public string Name { get; set; }
                public int Value { get; set; }
            }
            public string Name { get; set; }
            public int Value { get; set; }
            public List<Inner> Properties { get; set; }

        }

        private AppOptions _options;
        private IHttpContextAccessor _httpContextAccessor;
        private ILogger<ConsentController> _logger;

        private const string ServiceName = "sample";
      //  private const string ScopeBaseUrl = "https://www.samplecompanyapis.com/auth";
        public ConsentController(
            IOptions<AppOptions> options,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ConsentController> logger)
        {
            _options = options.Value;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpGet(".well-known/consent-configuration")]
        public async Task<ConsentDiscoveryDocument> GetDiscoveryDocumentAsync(string tenant)
        {
            if(string.IsNullOrWhiteSpace(tenant)) throw new ArgumentException($"tenant is null",nameof(tenant));
            if(tenant.Length > 32) throw new ArgumentException($"tenant must be <= 32", nameof(tenant));

            return new ConsentDiscoveryDocument
            {
                AuthorizeEndpoint = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/{tenant}/api/consent/authorize",
                ScopesSupported = new List<string> 
                {
                    $"{_options.ScopeBaseUrl}/{tenant}",  // full access
                    $"{_options.ScopeBaseUrl}/{tenant}.readonly",
                    $"{_options.ScopeBaseUrl}/{tenant}.modify"
                },
                AuthorizationType = Constants.AuthorizationTypes.SubjectAndScopes
            };
        }
        [HttpPost("authorize")]
        public async Task<IActionResult> PostAuthorizeAsync([FromBody] ConsentAuthorizeRequest authorizeRequest)
        {
            
            var authorizeResponse = new ConsentAuthorizeResponse
            {
                Authorized = false,
                Subject = authorizeRequest.Subject 
            };
            if (string.IsNullOrWhiteSpace(authorizeRequest.Subject)) 
            {
                authorizeResponse.Error = new ConsentBaseResponse.ConsentError
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "bad subject"
                };
                return Unauthorized(authorizeResponse); 
            }

            // we are a SubjectAndScopes controller so scopes have to be present;
            if (authorizeRequest.Scopes == null|| !authorizeRequest.Scopes.Any())
            {
                authorizeResponse.Error = new ConsentBaseResponse.ConsentError
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "No scopes where requested!"
                };
                return Unauthorized(authorizeResponse);
            }

            // check if user is in our database.
            //authorizeResponse.Authorized = authorizeRequest.Subject == "good" || authorizeRequest.Subject == "104758924428036663951" ;
            authorizeResponse.Authorized = true;  // accept all.
            if (authorizeResponse.Authorized)
            {
                authorizeResponse.Scopes = authorizeRequest.Scopes;
                authorizeResponse.Claims = new List<ConsentAuthorizeResponse.ConsentAuthorizeClaim>
                {
                    new ConsentAuthorizeResponse.ConsentAuthorizeClaim
                    {
                        Type = "geo_location",
                        Value = "Canada"
                    }
                };
                authorizeResponse.CustomPayload = new MyCustom
                {
                    Name = nameof(MyCustom), Value = 1234,
                    Properties =  new List<MyCustom.Inner>()
                    {
                        new MyCustom.Inner()
                        {
                            Name = GuidS,
                            Value = 1
                        },
                        new MyCustom.Inner()
                        {
                            Name = GuidS,
                            Value = 2
                        }
                    }
                };
            }
            else
            {
                authorizeResponse.Error = new ConsentBaseResponse.ConsentError
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "User is bad!"
                };
            }

            if (authorizeResponse.Authorized)
            {
                return Ok(authorizeResponse);
            }
            return Unauthorized(authorizeResponse);
        }
    }
}
