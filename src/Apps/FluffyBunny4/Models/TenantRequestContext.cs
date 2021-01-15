using System.Collections.Specialized;

namespace FluffyBunny4.Models
{
    public class TenantRequestContext
    {
        public string TenantName { get; set; }
        public string Issuer { get; set; }
        public ClientExtra Client { get; set; }
        public NameValueCollection FormCollection { get; set; }
    }
}