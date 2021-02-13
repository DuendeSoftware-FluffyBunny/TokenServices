using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyBunny.EntityFramework.Entities
{
    public class Certificate
    {
        public int Id { get; set; }
        public string SigningAlgorithm { get; set; }  // i.e. SecurityAlgorithms.RsaSha256
        public string PFXBase64 { get; set; }
        public DateTime NotBefore { get; set; }
        public DateTime Expiration { get; set; }
    }
}
