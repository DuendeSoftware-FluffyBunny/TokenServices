namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Models
{
    public class SelfManagedCertificatesOptions
    {
        public string Password { get; set; }
        public bool Enabled { get; set; }
        public string SigningAlgorithm { get; set; }
    }
}