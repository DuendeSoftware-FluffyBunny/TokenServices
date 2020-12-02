using System.Collections.Generic;

namespace FluffyBunny4.Azure.Configuration.CosmosDB
{
    /// <summary>
    ///     AppSettings CosmosDb Configuration Section.
    /// </summary>
    public class CosmosDbConfiguration
    {
        /// <summary>
        ///     URL EndPoint for CosmosDb Instance.
        /// </summary>
        public string EndPointUrl { get; set; }

        /// <summary>
        ///     Primary Key for CosmosDb Instance.
        /// </summary>
        public string PrimaryKey { get; set; }

        /// <summary>
        ///     Database name for the control data.
        /// </summary>
        public string DatabaseName { get; set; }
    }
}