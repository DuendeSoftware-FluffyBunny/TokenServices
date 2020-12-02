namespace FluffyBunny4.Services
{
    public interface IAzureKeyVaultECDsaSignatureProviderConfiguration
    {
        void SetKeyVaultECDsaKeyStore(IKeyVaultECDsaKeyStore keyVaultECDsaKeyStore);
    }
}
