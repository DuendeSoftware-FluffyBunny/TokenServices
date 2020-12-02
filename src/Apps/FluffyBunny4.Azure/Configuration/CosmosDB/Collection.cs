namespace FluffyBunny4.Azure.Configuration.CosmosDB
{
    /// <summary>
    ///     AppSettings CosmosDb Configuration Sub-Section (Collections).
    /// </summary>
    public class Collection
    {
        /// <summary>
        ///     Collection Name
        /// </summary>
        /// <value>
        ///     ApiResources
        ///     Clients
        ///     IdentityResources
        /// </value>
        public string CollectionName { get; set; }

        /// <summary>
        ///     The number of RU/sec to set for this collection.
        ///     <c>Default is 1000</c>
        /// </summary>
        public int ReserveUnits { get; set; } = 1000;
    }
}