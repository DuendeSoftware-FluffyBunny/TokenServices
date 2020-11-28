using FluffyBunny4.Models;
using FluffyBunny4.Models.Client;

namespace FluffyBunny4.Extensions
{
    public class ConsentDiscoveryDocumentRequest : ConsentProtocolRequest
    {

        /// <summary>
        /// Gets or sets the policy.
        /// </summary>
        /// <value>
        /// The policy.
        /// </value>
        public ConsentDiscoveryPolicy Policy { get; set; } = new ConsentDiscoveryPolicy();
    }
}
