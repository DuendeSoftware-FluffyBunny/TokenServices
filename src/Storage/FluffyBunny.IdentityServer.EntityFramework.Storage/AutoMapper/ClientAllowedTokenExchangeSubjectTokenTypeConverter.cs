using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluffyBunny.EntityFramework.Entities;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.AutoMapper
{
    public class ClientAllowedTokenExchangeSubjectTokenTypeConverter : IValueConverter<ICollection<AllowedTokenExchangeSubjectTokenType>, List<string>>
    {
        public List<string> Convert(ICollection<AllowedTokenExchangeSubjectTokenType> sourceMember, ResolutionContext context)
        {
            var query = from item in sourceMember
                select item.SubjectTokenType;
            return query.ToList();
        }
    }
}