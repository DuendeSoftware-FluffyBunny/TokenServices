namespace FluffyBunny.EntityFramework.Entities
{
    public class AllowedRevokeTokenTypeHint
    {
        public int Id { get; set; }
        public string TokenTypeHint { get; set; }

        public int ClientId { get; set; }
        public ClientExtra Client { get; set; }
    }
}