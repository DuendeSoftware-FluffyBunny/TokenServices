namespace TokenService.Models
{
    public class KeyVaultSigningOptions
    {
        public enum SigningTypes
        {
            KeyVaultCertificate,
            KeyVaultECDsaKey,
            KeyVaultRSAKey
        }
        public SigningTypes SigningType { get; set; }
        public string KeyVaultName { get; set; }
        public bool Enabled { get; set; } = false;
    }
}