namespace FluffyBunny.EntityFramework.Entities
{
    public class AllowedTokenExchangeExternalService
    {
        public int Id { get; set; }
        public string ExternalService { get; set; }

        public int ClientId { get; set; }
        public ClientExtra Client { get; set; }
    }
}