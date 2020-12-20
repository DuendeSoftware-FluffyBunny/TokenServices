using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace FluffyBunny4.AutoMapper
{
    public class AuthorizedScopesConverter : IValueConverter<IEnumerable<string>, IEnumerable<string>>
    {
        public IEnumerable<string> Convert(IEnumerable<string> sourceMember, ResolutionContext context)
        {
            if (sourceMember == null) return null;
            return sourceMember.ToList();
        }
    }
 
}