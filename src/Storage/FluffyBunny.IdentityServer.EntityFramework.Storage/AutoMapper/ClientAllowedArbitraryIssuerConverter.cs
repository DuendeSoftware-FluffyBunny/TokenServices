using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluffyBunny.EntityFramework.Entities;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.AutoMapper
{
    public class ClientAllowedArbitraryIssuerConverter : IValueConverter<ICollection<AllowedArbitraryIssuer>, List<string>>
    {
        public List<string> Convert(ICollection<AllowedArbitraryIssuer> sourceMember, ResolutionContext context)
        {
            var query = from item in sourceMember
                select item.Issuer;
            return query.ToList();
        }
    }
}