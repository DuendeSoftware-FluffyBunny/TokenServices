using Microsoft.Extensions.Options;

namespace FluffyBunny4.DotNetCore.Services.Defaults
{

    internal class DataProtection : IDataProtection
    {
        private DataProtectionOptions _options;

        public DataProtection(IOptions<DataProtectionOptions> options)
        {
            _options = options.Value;
        }

        public string UnprotectString(string cipherText)
        {
            return AesOperation.DecryptString(_options.Key, cipherText);
        }

        public string ProtectString(string plainText)
        {
            return AesOperation.EncryptString(_options.Key, plainText);
        }
    }
}
