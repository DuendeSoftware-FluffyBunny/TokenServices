using Azure.Security.KeyVault.Keys.Cryptography;
using FluffyBunny4.DotNetCore.Services;
using FluffyBunny4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Models;


namespace FluffyBunny4.Services
{
    public class AzureKeyVaultECDsaSignatureProvider :
        IAzureKeyVaultECDsaSignatureProviderConfiguration, ISignatureProvider
    {
        private ILogger _logger;
        private ISerializer _serializer;

        private IKeyVaultECDsaKeyStore _keyVaultECDsaKeyStore;

        public AzureKeyVaultECDsaSignatureProvider(
            IKeyVaultECDsaKeyStore eCDsaKeyInfo,
            ISerializer serializer,
            ILogger<AzureKeyVaultECDsaSignatureProvider> logger)
        {
            _logger = logger;
            _keyVaultECDsaKeyStore = eCDsaKeyInfo;
            _serializer = serializer;
        }

       

        public async Task<string> CreateJwtAsync(JwtSecurityToken token)
        {
            var ecdsaCache = await _keyVaultECDsaKeyStore.FetchCacheAsync();
            string algorithm = ecdsaCache.SecurityKeyInfos.First().SigningAlgorithm;
            var header = Base64UrlEncoder.Encode(_serializer.Serialize(
                new Dictionary<string, string>()
            {
                { JwtHeaderParameterNames.Alg, algorithm },
                { JwtHeaderParameterNames.Kid, ecdsaCache.CurrentKeyProperties.Version },
                { JwtHeaderParameterNames.Typ, "JWT" }
            }));
            var rawDataBytes = Encoding.UTF8.GetBytes(header + "." + token.EncodedPayload);
            byte[] hash;
            using (var hasher = CryptoHelper.GetHashAlgorithmForSigningAlgorithm(algorithm))
            {
                hash = hasher.ComputeHash(rawDataBytes);
            }
            var signResult = await ecdsaCache.CryptographyClient.SignAsync(
                new SignatureAlgorithm(algorithm), hash);

            var rawSignature = Base64UrlTextEncoder.Encode(signResult.Signature);

            return $"{header}.{token.EncodedPayload}.{rawSignature}";

        }

        public async Task<string> CreateJwtAsync(Token token)
        {
            var ecdsaCache = await _keyVaultECDsaKeyStore.FetchCacheAsync();
 
            var jwtSecurityToken = token.CreateJwtSecurityToken(null, _logger);
            var jwtToken = await CreateJwtAsync(jwtSecurityToken);

            return jwtToken;
        }
        

        public void SetKeyVaultECDsaKeyStore(IKeyVaultECDsaKeyStore keyVaultECDsaKeyStore)
        {
            _keyVaultECDsaKeyStore = keyVaultECDsaKeyStore;
        }


    }
}
