using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluffyBunny.EntityFramework.Entities;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.AutoMapper
{
    public class ClientAllowedRevokeTokenTypeHintConverter : IValueConverter<ICollection<AllowedRevokeTokenTypeHint>, List<string>>
    {
        public List<string> Convert(ICollection<AllowedRevokeTokenTypeHint> sourceMember, ResolutionContext context)
        {
            var query = from item in sourceMember
                select item.TokenTypeHint;
            return query.ToList();
        }
    }
}