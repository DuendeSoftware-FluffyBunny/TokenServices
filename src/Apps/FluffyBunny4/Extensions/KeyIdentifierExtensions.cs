using Duende.IdentityServer.Configuration;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Tokens;

namespace FluffyBunny4.Extensions
{
    public static class KeyIdentifierExtensions
    {
        public static string KidToHash(this KeyIdentifier keyIdentifier)
        {
            return keyIdentifier.Identifier.LongKidStringToHash();
        }
        public static string LongKidStringToHash(this string kid)
        {
            return Base64UrlEncoder.Encode(CryptoHelper.CreateHashClaimValue(kid, "RSA256"));
        }
    }
}
