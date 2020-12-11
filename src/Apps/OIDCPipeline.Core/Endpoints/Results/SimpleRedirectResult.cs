// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OIDCPipeline.Core.Hosting;

namespace OIDCPipeline.Core.Endpoints.Results
{
     
    internal class SimpleRedirectResult : IEndpointResult
    {
        private string _redirectUri;

        public SimpleRedirectResult(string redirectUri)
        {
            _redirectUri = redirectUri;
        }
        public async Task ExecuteAsync(HttpContext context)
        {
            context.Response.SetNoCache();
            context.Response.Redirect(_redirectUri);
        }
    }
}