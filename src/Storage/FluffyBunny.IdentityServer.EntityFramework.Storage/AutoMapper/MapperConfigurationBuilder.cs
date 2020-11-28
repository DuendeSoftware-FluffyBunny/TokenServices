using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using FluffyBunny.IdentityServer.EntityFramework.Storage.Entities;
using FluffyBunny4.DotNetCore.Extensions;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.AutoMapper
{
    public static class MapperConfigurationBuilder
    {
        public static IMapper BuidIgnoreBaseMapper => BuidIgnoreBaseMapperConfiguration.CreateMapper();
        public static MapperConfiguration BuidIgnoreBaseMapperConfiguration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ExternalService, ExternalService>()
                .Ignore(record => record.Id);

            cfg.CreateMap<Tenant, Tenant>()
                .Ignore(record => record.Id);

            cfg.CreateMap<ClientExtra, ClientExtra>()
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
        });

        public static IMapper BuidOneToOneMapper => BuidOneToOneMapperConfiguration.CreateMapper();
        public static MapperConfiguration BuidOneToOneMapperConfiguration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ExternalService, ExternalService>();

            cfg.CreateMap<Tenant, Tenant>();
            cfg.CreateMap<ClientExtra, ClientExtra>();

        });
    }
}
