namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Entities
{
    public class ExternalService
    {
        public int Id { get; set; }
        public string Name { get; set; }  // service name
        public string Description { get; set; }
        public string Authority { get; set; }

    }
}