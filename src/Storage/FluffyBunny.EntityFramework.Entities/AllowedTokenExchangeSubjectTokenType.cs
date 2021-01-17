namespace FluffyBunny.EntityFramework.Entities
{
    public class AllowedTokenExchangeSubjectTokenType
    {
        public int Id { get; set; }
        public string SubjectTokenType { get; set; }

        public int ClientId { get; set; }
        public ClientExtra Client { get; set; }
    }
}