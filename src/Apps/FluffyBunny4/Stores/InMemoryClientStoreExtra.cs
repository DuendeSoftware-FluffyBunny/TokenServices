using FluffyBunny4.Models;
using FluffyBunny4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;

namespace FluffyBunny4.Stores
{
    public class InMemoryClientStoreExtra : IClientStore
    {
        private readonly IEnumerable<Client> _clients;
        private readonly IScopedTenantRequestContext _scopedTenantRequestContext;

        public InMemoryClientStoreExtra(
            IEnumerable<Client> clients,
            IScopedTenantRequestContext scopedTenantRequestContext
            ) 
        {
            _clients = clients;
            _scopedTenantRequestContext = scopedTenantRequestContext;
        }


        /// <summary>
        /// Finds a client by id
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <returns>
        /// The client
        /// </returns>
        public Task<Client> FindClientByIdAsync(string clientId)
        {
            return FindClientByIdAsync(clientId, _scopedTenantRequestContext.TenantId);
        }

        public Task<Client> FindClientByIdAsync(string clientId, string tenantId)
        {
            var query =
                from client in _clients
                let c = client as ClientExtra
                where client.ClientId == clientId && c.TenantName == tenantId
                select client;
            return Task.FromResult(query.SingleOrDefault());
        }
    }
}
