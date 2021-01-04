using FluffyBunny4.Models;
using FluffyBunny4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using FluffyBunny4.DotNetCore.Services;

namespace FluffyBunny4.Stores
{
    public class InMemoryClientStoreExtra : IClientStore
    {
        private readonly IScopedContext<TenantRequestContext> _scopedTenantRequestContext;
        private readonly IEnumerable<Client> _clients;

        public InMemoryClientStoreExtra(
            IScopedContext<TenantRequestContext> scopedTenantRequestContext,
            IEnumerable<Client> clients
            )
        {
            _scopedTenantRequestContext = scopedTenantRequestContext;
            _clients = clients;
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
            return FindClientByIdAsync(clientId, _scopedTenantRequestContext.Context.TenantName);
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
