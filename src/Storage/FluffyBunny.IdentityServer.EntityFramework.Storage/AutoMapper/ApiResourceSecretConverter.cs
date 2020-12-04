using System.Collections.Generic;
using AutoMapper;
using Duende.IdentityServer.EntityFramework.Entities;
using Secret = Duende.IdentityServer.Models.Secret;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.AutoMapper
{
    public class ApiResourceSecretConverter : IValueConverter<List<Duende.IdentityServer.EntityFramework.Entities.ApiResourceSecret>, ICollection<Duende.IdentityServer.Models.Secret>>
    {
        public ICollection<Duende.IdentityServer.Models.Secret> Convert(List<Duende.IdentityServer.EntityFramework.Entities.ApiResourceSecret> sourceMember, ResolutionContext context)
        {
            var secrets = new HashSet<Duende.IdentityServer.Models.Secret>();
            foreach (var item in sourceMember)
            {
                secrets.Add(new Duende.IdentityServer.Models.Secret()
                {
                    Description = item.Description,
                    Expiration = item.Expiration,
                    Type = item.Type,
                    Value = item.Value
                });
            }
            return secrets;
        }
    }
}