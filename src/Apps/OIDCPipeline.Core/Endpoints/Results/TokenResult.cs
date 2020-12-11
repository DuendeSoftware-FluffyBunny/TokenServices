// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Common;
using FluffyBunny4.DotNetCore.Services;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using OIDCPipeline.Core.Endpoints.ResponseHandling;
using OIDCPipeline.Core.Extensions;
using OIDCPipeline.Core.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OIDCPipeline.Core.Endpoints.Results
{
    internal class TokenResult : IEndpointResult
    {
        private ISerializer _serializer;

        public TokenResponse Response { get; set; }

        public TokenResult(TokenResponse response, ISerializer serializer)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            _serializer = serializer;
            Response = response;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            context.Response.SetNoCache();

            var dto = new ResultDto
            {
                id_token = Response.IdentityToken,
                access_token = Response.AccessToken,
                refresh_token = Response.RefreshToken,
                expires_in = Response.AccessTokenLifetime,
                token_type = OidcConstants.TokenResponse.BearerTokenType,
                custom = Response.Custom
            };
            var json = _serializer.Serialize(dto);
            await context.Response.WriteJsonAsync(json);

        }

        internal class ResultDto
        {
            public string id_token { get; set; }
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
            public string refresh_token { get; set; }
            public Dictionary<string, object> custom { get; set; }
        }
    }
}