using FluffyBunny4.Configuration;

namespace FluffyBunny4.Services
{
    public interface IKeyVaultECDsaKeyStoreConfiguration
    {
        void SetOptions(KeyVaultStoreOptions options);
    }
}
