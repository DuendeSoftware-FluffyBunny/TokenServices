using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Duende.IdentityServer.EntityFramework.Entities;
using Secret = Duende.IdentityServer.Models.Secret;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.AutoMapper
{
    public class ClientSecretsConverter : IValueConverter<ICollection<ClientSecret>, List<Secret>>
    {
        public List<Secret> Convert(ICollection<ClientSecret> sourceMember, ResolutionContext context)
        {
            var query = from item in sourceMember
                let c = context.Mapper.Map<Secret>(item)
                select c;
            return query.ToList();
        }
    }
}
