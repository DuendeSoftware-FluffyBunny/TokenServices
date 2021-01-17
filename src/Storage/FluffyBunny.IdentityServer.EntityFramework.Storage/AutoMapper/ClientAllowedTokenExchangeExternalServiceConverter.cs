using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluffyBunny.EntityFramework.Entities;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.AutoMapper
{
    public class ClientAllowedTokenExchangeExternalServiceConverter : IValueConverter<ICollection<AllowedTokenExchangeExternalService>, List<string>>
    {
        public List<string> Convert(ICollection<AllowedTokenExchangeExternalService> sourceMember, ResolutionContext context)
        {
            var query = from item in sourceMember
                select item.ExternalService;
            return query.ToList();
        }
    }
}