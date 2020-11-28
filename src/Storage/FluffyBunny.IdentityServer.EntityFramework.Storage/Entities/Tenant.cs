namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Entities
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
}