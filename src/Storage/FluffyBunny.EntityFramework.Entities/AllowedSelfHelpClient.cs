namespace FluffyBunny.EntityFramework.Entities
{
    public class AllowedSelfHelpClient
    {
        public int Id { get; set; }
        public int SelfHelpUserId { get; set; }
        public string ClientId { get; set; }
        public SelfHelpUser SelfHelpUser { get; set; }
    }
}