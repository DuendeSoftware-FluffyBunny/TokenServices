
using Duende.IdentityServer.Models;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FluffyBunny4.Services
{
    public class AzureKeyVaultSignatureProvider : ISignatureProvider
    {
        private ILogger _logger;
        private IKeyVaultCertificateStore _jwksUriHandler;
        private KeyVaultClient _keyVaultClient;
        private SHA256 _hashAlgorithm;
        private string _algorithm;

        public AzureKeyVaultSignatureProvider(ILogger<AzureKeyVaultSignatureProvider> logger,
            IKeyVaultCertificateStore jwksUriHandler)
        {
            _logger = logger;
            _jwksUriHandler = jwksUriHandler;
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            _keyVaultClient = new KeyVaultClient(
                new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            _hashAlgorithm = SHA256.Create();
            _algorithm = JsonWebKeySignatureAlgorithm.RS256;
        }
        public async Task<byte[]> SignAsync(byte[] input)
        {
            try
            {
                var digest = _hashAlgorithm.ComputeHash(input);
                var cacheData = await _jwksUriHandler.FetchCacheDataAsync();
                var result = await _keyVaultClient.SignAsync(
                    cacheData.KeyIdentifier.Identifier,
                    _algorithm, digest);
                return result.Result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<string> CreateJwtAsync(JwtSecurityToken token)
        {
            var cacheData = await _jwksUriHandler.FetchCacheDataAsync();
            var header = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(new Dictionary<string, string>()
            {
                { JwtHeaderParameterNames.Alg, "RS256" },
                { JwtHeaderParameterNames.Kid, cacheData.KidHash },
                { JwtHeaderParameterNames.Typ, "JWT" }
            }));

            var rawDataBytes = Encoding.UTF8.GetBytes(header + "." + token.EncodedPayload);

            var signedData = await SignAsync(rawDataBytes);
            var rawSignature = Base64UrlEncoder.Encode(signedData);

            return $"{header}.{token.EncodedPayload}.{rawSignature}";
        }

        public Task<string> CreateJwtAsync(Token token)
        {
            throw new NotImplementedException();
        }
    }
}
