namespace FluffyBunny4.Models
{
    public class OpenIdConnectAuthority
    {
        public string Name { get; set; }  // service name
        public string Description { get; set; }
        public string Authority { get; set; }
        public bool Enabled { get; set; }
    }
}