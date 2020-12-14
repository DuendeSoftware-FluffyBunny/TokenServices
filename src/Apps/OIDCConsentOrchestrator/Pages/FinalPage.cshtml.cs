using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
 
using FluffyBunny4.DotNetCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OIDCPipeline.Core;
using OIDCPipeline.Core.Endpoints.ResponseHandling;

namespace OIDCConsentOrchestrator.Pages
{
    [Authorize]
    public class FinalPageModel : PageModel
    {
        class SomeObject
        {
            public string Name { get; set; }
        }
        class Custom
        {
            public string Name { get; set; }
            public List<int> Numbers { get; set; }
            public List<string> Strings { get; set; }
            public List<SomeObject> SomeObjects { get; set; }
            public SomeObject SomeObject { get; set; }
            public Dictionary<string,object> Temp { get; set; }


        }
        private IOIDCPipeLineKey _oidcPipeLineKey;
        private SignInManager<IdentityUser> _signInManager;
        private IOIDCResponseGenerator _oidcResponseGenerator;
        private IOIDCPipelineStore _oidcPipelineStore;
        private readonly ISerializer _serializer;
        private readonly ILogger<FinalPageModel> _logger;

        public FinalPageModel(
            IOIDCPipeLineKey oidcPipeLineKey,
            SignInManager<IdentityUser> signInManager,
            IOIDCResponseGenerator oidcResponseGenerator,
            IOIDCPipelineStore oidcPipelineStore,
            ISerializer serializer,
            ILogger<FinalPageModel> logger)
        {
            _oidcPipeLineKey = oidcPipeLineKey;
            _signInManager = signInManager;
            _oidcResponseGenerator = oidcResponseGenerator;
            _oidcPipelineStore = oidcPipelineStore;
            _serializer = serializer;
            _logger = logger;
        }

        public List<Claim> Claims { get; set; }
        public DownstreamAuthorizeResponse IdTokenResponse { get; private set; }

        public async Task OnGetAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                string nonce = _oidcPipeLineKey.GetOIDCPipeLineKey();
                IdTokenResponse = await _oidcPipelineStore.GetDownstreamIdTokenResponseAsync(nonce);

                Claims = Request.HttpContext.User.Claims.ToList();
            }
        }
        public async Task<IActionResult> OnPostWay2(string data)
        {
            string key = _oidcPipeLineKey.GetOIDCPipeLineKey();
            var originalIdTokenRequest = await _oidcPipelineStore.GetOriginalIdTokenRequestAsync(key);
            var tempCustom = await _oidcPipelineStore.GetTempCustomObjectsAsync(key);
            var custom = new Custom
            {
                Name = "Bugs Bunny",
                Numbers = new List<int>() { 1, 2, 3 },
                Strings = new List<string>() { "a", "bb", "ccc" },
                SomeObject = new SomeObject { Name = "Daffy Duck" },
                SomeObjects = new List<SomeObject>()
                {
                    new SomeObject { Name = "Daisy Duck"},
                    new SomeObject { Name = "Porky Pig"},
                },
                Temp = tempCustom
            };
            var json = _serializer.Serialize(custom);

            await _oidcPipelineStore.StoreDownstreamCustomDataAsync(key, new Dictionary<string, object> {
                { "prodInstance",Guid.NewGuid()},
                { "extraStuff",custom},
                {"originalRequest" ,originalIdTokenRequest.Raw}
            });

            var result = await _oidcResponseGenerator.CreateAuthorizeResponseActionResultAsync(key, true);
            await _signInManager.SignOutAsync();// we don't want our loggin hanging around
            return result;

        }
    }
}
