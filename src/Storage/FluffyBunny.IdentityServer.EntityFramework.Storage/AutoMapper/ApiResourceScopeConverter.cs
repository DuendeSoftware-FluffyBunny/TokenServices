using System.Collections.Generic;
using AutoMapper;
using Duende.IdentityServer.EntityFramework.Entities;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.AutoMapper
{
    public class ApiResourceScopeConverter : IValueConverter<List<ApiResourceScope>, ICollection<string>>
    {
        public ICollection<string> Convert(List<Duende.IdentityServer.EntityFramework.Entities.ApiResourceScope> sourceMember, ResolutionContext context)
        {
            List<string> scopes = new List<string>();
            foreach (var item in sourceMember)
            {
                scopes.Add(item.Scope);
            }
            return scopes;
        }
    }
}