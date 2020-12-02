using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using FluffyBunny4.DotNetCore.Extensions;
 

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.AutoMapper
{
    public static class MapperConfigurationBuilder
    {
        public static IMapper BuidIgnoreBaseMapper => BuidIgnoreBaseMapperConfiguration.CreateMapper();
        public static MapperConfiguration BuidIgnoreBaseMapperConfiguration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.ExternalService, FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.ExternalService>()
                .Ignore(record => record.Id);

            cfg.CreateMap<FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.Tenant, FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.Tenant>()
                .Ignore(record => record.Id);

            cfg.CreateMap<FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.ClientExtra, FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.ClientExtra>()
                .Ignore(record => record.Id)
                .Ignore(record => record.ClientSecrets)
                .Ignore(record => record.AllowedGrantTypes)
                .Ignore(record => record.RedirectUris)
                .Ignore(record => record.PostLogoutRedirectUris)
                .Ignore(record => record.AllowedScopes)
                .Ignore(record => record.IdentityProviderRestrictions)
                .Ignore(record => record.Claims)
                .Ignore(record => record.AllowedCorsOrigins)
                .Ignore(record => record.Properties);

            cfg.CreateMap<Duende.IdentityServer.EntityFramework.Entities.ApiResource, Duende.IdentityServer.EntityFramework.Entities.ApiResource>()
                .Ignore(record => record.Id)
                .Ignore(record => record.Properties)
                .Ignore(record => record.Scopes)
                .Ignore(record => record.Secrets)
                .Ignore(record => record.UserClaims) ;

        });

        public static IMapper BuidOneToOneMapper => BuidOneToOneMapperConfiguration.CreateMapper();
        public static MapperConfiguration BuidOneToOneMapperConfiguration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.ExternalService, FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.ExternalService>();
            cfg.CreateMap<FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.ExternalService, FluffyBunny4.Models.ExternalService>();

            cfg.CreateMap<FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.Tenant, FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.Tenant>();
            cfg.CreateMap<FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.Tenant, FluffyBunny4.Models.TenantHandle>();

            cfg.CreateMap<FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.ClientExtra, FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.ClientExtra>();
            cfg.CreateMap<FluffyBunny.IdentityServer.EntityFramework.Storage.Entities.ClientExtra, FluffyBunny4.Models.ClientExtra>();


            cfg.CreateMap<Duende.IdentityServer.EntityFramework.Entities.ApiResource, Duende.IdentityServer.EntityFramework.Entities.ApiResource>();
            cfg.CreateMap<Duende.IdentityServer.EntityFramework.Entities.ApiResource, Duende.IdentityServer.Models.ApiResource>();

        });
    }
}
