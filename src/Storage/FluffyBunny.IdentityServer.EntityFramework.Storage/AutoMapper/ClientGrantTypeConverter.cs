using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Duende.IdentityServer.EntityFramework.Entities;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.AutoMapper
{
    public class ClientGrantTypeConverter : IValueConverter<ICollection<ClientGrantType>, List<string>>
    {
        public List<string> Convert(ICollection<ClientGrantType> sourceMember, ResolutionContext context)
        {
            var query = from item in sourceMember
                select item.GrantType;
            return query.ToList();
        }
    }
}