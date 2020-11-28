namespace TestWebApp.Models
{
    public class AppOptions
    {
        public enum DatabaseTypes
        {
            Postgres,
            CosmosDB,
            InMemory,
            SqlServer
        }
        public DatabaseTypes DatabaseType { get; set; }
    
    }
}
