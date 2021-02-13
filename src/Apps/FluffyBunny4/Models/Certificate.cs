using System;

namespace FluffyBunny4.Models
{
    public class Certificate
    {
        public string SigningAlgorithm { get; set; }  // i.e. SecurityAlgorithms.RsaSha256
        public string PFXBase64 { get; set; }
        public DateTime NotBefore { get; set; }
        public DateTime Expiration { get; set; }
        public string JWK { get; set; }
    }
}