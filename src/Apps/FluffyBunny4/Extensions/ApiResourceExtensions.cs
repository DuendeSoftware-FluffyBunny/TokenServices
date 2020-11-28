using System.Collections.Generic;
using System.Linq;
using Duende.IdentityServer.Models;

namespace FluffyBunny4.Extensions
{
    public static class ApiResourceExtensions
    {
        public static List<string> ToScopeNames(this List<ApiResource> self)
        {
            var query = from item in self
                        select item.Name;
            return query.ToList();
        }
    }
}
