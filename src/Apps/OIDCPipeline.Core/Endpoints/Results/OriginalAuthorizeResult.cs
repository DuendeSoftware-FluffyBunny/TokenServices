// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OIDCPipeline.Core.Hosting;

namespace OIDCPipeline.Core.Endpoints.Results
{
     
    internal class OriginalAuthorizeResult : IEndpointResult
    {
        private IOIDCPipeLineKey _oidcPipeLineKey;
        private string _redirectUri;
        private string _key;

      

        public OriginalAuthorizeResult(IOIDCPipeLineKey oidcPipeLineKey, string redirectUrl, string key)
        {
            _oidcPipeLineKey = oidcPipeLineKey;
            _redirectUri = redirectUrl;
            _key = key;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            context.Response.SetNoCache();
            if (!string.IsNullOrWhiteSpace(_key))
            {
                _oidcPipeLineKey.SetOIDCPipeLineKey(_key);
            }
            context.Response.Redirect(_redirectUri);
        }
    }
}