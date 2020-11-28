using FluffyBunny4.Models;
using System.Collections.Generic;
using System.Linq;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Extensions
{
    public static class ApiResourceRecordExtensions
    {
         
        public static List<ApiResource> ToApiResources(this List<ApiResourceRecord> self)
        {
            List<ApiResource> apiResources = new List<ApiResource>();
            foreach (var apiResourceRecord in self)
            {
                // no matter what add the api resource as a scope
                List<string> scopes = new List<string> { apiResourceRecord.Name };
                if (apiResourceRecord.Scopes != null)
                {
                    var prePend = string.IsNullOrWhiteSpace(apiResourceRecord.ScopeNameSpace)
                        ? $"{apiResourceRecord.Name}."
                        : $"{apiResourceRecord.ScopeNameSpace}.";
                    foreach (var scopeRecord in apiResourceRecord.Scopes)
                    {
                        scopes.Add("{prePend}{scopeRecord.Name}");
                    }
                }
                List<Secret> apiSecrets = new List<Secret>();
                if (apiResourceRecord.Secrets != null)
                {
                    foreach (var secret in apiResourceRecord.Secrets)
                    {
                        apiSecrets.Add(new Secret(secret.Sha256()));
                    }


                }
                apiResources.Add(new ApiResource(apiResourceRecord.Name)
                {
                    Scopes = scopes,
                    ApiSecrets = apiSecrets
                });
            }
            return apiResources;
        }
        public static ApiResource ToApiResource(this ApiResourceRecord apiResourceRecord)
        {
            List<string> scopes = new List<string> { apiResourceRecord.Name };
            if (apiResourceRecord.Scopes != null)
            {
                var prePend = string.IsNullOrWhiteSpace(apiResourceRecord.ScopeNameSpace)
                    ? $"{apiResourceRecord.Name}."
                    : $"{apiResourceRecord.ScopeNameSpace}.";
                foreach (var scopeRecord in apiResourceRecord.Scopes)
                {
                    scopes.Add($"{prePend}{scopeRecord.Name}");
                }
            }
            List<Secret> apiSecrets = new List<Secret>();
            if (apiResourceRecord.Secrets != null)
            {
                foreach (var secret in apiResourceRecord.Secrets)
                {
                    apiSecrets.Add(new Secret(secret.Sha256()));
                }


            }
            var apiResource = new ApiResource(apiResourceRecord.Name)
            {
                Scopes = scopes,
                ApiSecrets = apiSecrets
            };

            return apiResource;
        }
    }
}
