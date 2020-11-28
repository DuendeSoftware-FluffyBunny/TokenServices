using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluffyBunny.Admin.Model
{
    public class AppOptions
    {
        public class AuthAndSessionCookiesContainer
        {
            public int TTL { get; set; }
        }

        public enum DatabaseTypes
        {
            Postgres,
            CosmosDB,
            SqlServer,
            InMemory
        }

        public DatabaseTypes DatabaseType { get; set; }
        public AuthAndSessionCookiesContainer AuthAndSessionCookies { get; set; }
    }
}
