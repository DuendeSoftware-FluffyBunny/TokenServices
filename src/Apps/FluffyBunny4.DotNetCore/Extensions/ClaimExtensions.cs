using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace FluffyBunny4.DotNetCore.Extensions
{
    public static class ClaimExtensions
    {
        public static IEnumerable<Claim> GetClaimsByType(this List<Claim> claims, string type)
        {
            var query = from item in claims
                        where item.Type == type
                        select item;
            return query;
        }
        public static IEnumerable<Claim> GetClaimsByType(this IEnumerable<Claim> claims, string type)
        {
            var query = from item in claims
                        where item.Type == type
                        select item;
            return query;
        }
    }
}
