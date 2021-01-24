using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluffyBunny.EntityFramework.Entities;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.AutoMapper
{
    public class SelfHelpUserAllowedClientConverter : IValueConverter<ICollection<AllowedSelfHelpClient>, List<string>>
    {
        public List<string> Convert(ICollection<AllowedSelfHelpClient> sourceMember, ResolutionContext context)
        {
            var query = from item in sourceMember
                select item.ClientId;
            return query.ToList();
        }
    }
}