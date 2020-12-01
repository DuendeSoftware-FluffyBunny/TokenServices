using System;
using System.ComponentModel.DataAnnotations;

namespace FluffyBunny.Admin.Model
{
    public class SecretModel
    {
        public class ExpirationTypes
        {
            public const string DoNotChange = "Do Not Change";
            public const string ExpireIt = "Expire It";
            public const string Never = "Never";
            public const string OneYear = "One Year";
        }

        
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Secret")]
        public string Value { get; set; }

        [Required]
        [Display(Name = "Expiration")]
        public string SecretExpiration { get; set; }
        public string[] SecretExpirations = new[] { ExpirationTypes.ExpireIt
            , ExpirationTypes.Never, ExpirationTypes.OneYear };

        public string[] EditSecretExpirations = new[] {
            ExpirationTypes.DoNotChange,
            ExpirationTypes.ExpireIt,
            ExpirationTypes.Never,
            ExpirationTypes.OneYear };
        public DateTime? CurrentExpiration { get; set; }
        public bool Reset { get; set; }
        public string SecretMasked { get; set; }

    }
}