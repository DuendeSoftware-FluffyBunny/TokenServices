using FluffyBunny.EntityFramework.Entities;
 
namespace FluffyBunny.EntityFramework.Entities
{
    public class AllowedArbitraryIssuer
    {
        public int Id { get; set; }
        public string Issuer { get; set; }

        public int ClientId { get; set; }
        public ClientExtra Client { get; set; }
    }
}