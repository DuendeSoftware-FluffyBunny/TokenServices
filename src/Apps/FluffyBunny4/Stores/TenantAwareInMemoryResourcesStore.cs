using AutoMapper;
using FluffyBunny4.Models;
using FluffyBunny4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using FluffyBunny4.DotNetCore.Services;

namespace FluffyBunny4.Stores
{
    public class TenantAwareInMemoryResourcesStore: IResourceStore
    {
        private IScopedContext<TenantContext> _scopedTenantContext;
        private IMapper _mapper;
        private readonly IEnumerable<IdentityResource> _identityResources;
        private readonly IEnumerable<TenantApiResourceHandle> _apiResources;
        private readonly IEnumerable<TenantApiScopeHandle> _apiScopes;

        public TenantAwareInMemoryResourcesStore(
            IScopedContext<TenantContext> scopedTenantContext,
            IMapper mapper,
            IEnumerable<IdentityResource> identityResources = null,
            IEnumerable<TenantApiResourceHandle> apiResources = null,
            IEnumerable<TenantApiScopeHandle> apiScopes = null)
        {
            _scopedTenantContext = scopedTenantContext;
            _mapper = mapper;


            if (identityResources?.HasDuplicates(m => m.Name) == true)
            {
                throw new ArgumentException("Identity resources must not contain duplicate names");
            }

            if (apiResources?.HasDuplicates(m => $"{m.Name}.{m.TenantId}") == true)
            {
                throw new ArgumentException("Api resources must not contain duplicate names");
            }

            if (apiScopes?.HasDuplicates(m => $"{m.Name}.{m.TenantId}") == true)
            {
                throw new ArgumentException("Scopes must not contain duplicate names");
            }



            _identityResources = identityResources ?? Enumerable.Empty<IdentityResource>();
            _apiResources = apiResources ?? Enumerable.Empty<TenantApiResourceHandle>();
            _apiScopes = apiScopes ?? Enumerable.Empty<TenantApiScopeHandle>();
        }
        /// <inheritdoc/>
        public Task<Resources> GetAllResourcesAsync()
        {
            var queryApiScopes =
              from x in _apiScopes
              where x.TenantId == _scopedTenantContext.Context.TenantName
              let xx = _mapper.Map<ApiScope>(x)
              select xx;
            var queryApiResources =
             from x in _apiResources
             where x.TenantId == _scopedTenantContext.Context.TenantName
             let xx = _mapper.Map<ApiResource>(x)
             select xx;
            var result = new Resources(_identityResources, queryApiResources, queryApiScopes);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            if (apiResourceNames == null) throw new ArgumentNullException(nameof(apiResourceNames));

            var query = from a in _apiResources
                        where apiResourceNames.Contains(a.Name) && a.TenantId == _scopedTenantContext.Context.TenantName
                        let xx = _mapper.Map<ApiResource>(a)
                        select xx;
            return Task.FromResult(query);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) throw new ArgumentNullException(nameof(scopeNames));

            var identity = from i in _identityResources
                           where scopeNames.Contains(i.Name)
                           select i;

            return Task.FromResult(identity);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
          
            if (scopeNames == null) throw new ArgumentNullException(nameof(scopeNames));

            var query = from a in _apiResources
                        where a.Scopes.Any(x => scopeNames.Contains(x)) && a.TenantId == _scopedTenantContext.Context.TenantName
                        let xx = _mapper.Map<ApiResource>(a)
                        select xx;
        
            return Task.FromResult(query);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) throw new ArgumentNullException(nameof(scopeNames));

            var query =
                from x in _apiScopes
                where scopeNames.Contains(x.Name) && x.TenantId == _scopedTenantContext.Context.TenantName
                let xx = _mapper.Map<ApiScope>(x)
                select xx;
            return Task.FromResult(query);
        }
    }
}
