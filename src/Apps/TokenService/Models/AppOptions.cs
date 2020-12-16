using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenService.Models
{
    public class AppOptions
    {
        public enum DatabaseTypes
        {
            Postgres,
            CosmosDB,
            InMemory,
            SqlServer,
            EntityFramework
        }
        public DatabaseTypes DatabaseType { get; set; }
        public DatabaseTypes OperationalStoreType { get; set; }
        public bool DisableHttpRedirect { get; set; }

    }
}
