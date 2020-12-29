using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Duende.IdentityServer.Models;
using FluffyBunny4.DotNetCore.Extensions;


namespace FluffyBunny.IdentityServer.EntityFramework.Storage.AutoMapper
{
    public static class MapperConfigurationBuilder
    {
        public static IMapper BuidIgnoreBaseMapper => BuidIgnoreBaseMapperConfiguration.CreateMapper();

        public static MapperConfiguration BuidIgnoreBaseMapperConfiguration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<FluffyBunny.EntityFramework.Entities.ExternalService,
                    FluffyBunny.EntityFramework.Entities.ExternalService>()
                .Ignore(record => record.Id);

            cfg.CreateMap<FluffyBunny.EntityFramework.Entities.Tenant,
                    FluffyBunny.EntityFramework.Entities.Tenant>()
                .Ignore(record => record.Id);

            cfg.CreateMap<FluffyBunny.EntityFramework.Entities.ClientExtra,
                    FluffyBunny.EntityFramework.Entities.ClientExtra>()
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

            cfg.CreateMap<Duende.IdentityServer.EntityFramework.Entities.ApiResource,
                    Duende.IdentityServer.EntityFramework.Entities.ApiResource>()
                .Ignore(record => record.Id)
                .Ignore(record => record.Properties)
                .Ignore(record => record.Scopes)
                .Ignore(record => record.Secrets)
                .Ignore(record => record.UserClaims);

        });

        public static IMapper BuidOneToOneMapper => BuidOneToOneMapperConfiguration.CreateMapper();

        public static MapperConfiguration BuidOneToOneMapperConfiguration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<FluffyBunny.EntityFramework.Entities.ExternalService,
                FluffyBunny.EntityFramework.Entities.ExternalService>();
            cfg.CreateMap<FluffyBunny.EntityFramework.Entities.ExternalService,
                FluffyBunny4.Models.ExternalService>();

            cfg.CreateMap<FluffyBunny.EntityFramework.Entities.Tenant,
                FluffyBunny.EntityFramework.Entities.Tenant>();
            cfg.CreateMap<FluffyBunny.EntityFramework.Entities.Tenant,
                FluffyBunny4.Models.TenantHandle>();



            cfg.CreateMap<Duende.IdentityServer.EntityFramework.Entities.ClientSecret,
                Duende.IdentityServer.Models.Secret>();
            cfg.CreateMap<Duende.IdentityServer.EntityFramework.Entities.ApiResourceSecret,
                Duende.IdentityServer.Models.Secret>();


            cfg.CreateMap<FluffyBunny.EntityFramework.Entities.ClientExtra,
                FluffyBunny.EntityFramework.Entities.ClientExtra>();
            cfg.CreateMap<FluffyBunny.EntityFramework.Entities.ClientExtra,
                    FluffyBunny4.Models.ClientExtra>()
                .ForMember(dest => dest.AllowedArbitraryIssuers, opt => opt.ConvertUsing(new ClientAllowedArbitraryIssuerConverter()))
                .ForMember(dest => dest.AllowedScopes, opt => opt.ConvertUsing(new ClientScopeConverter()))
                .ForMember(dest => dest.AllowedGrantTypes, opt => opt.ConvertUsing(new ClientGrantTypeConverter()));


            cfg.CreateMap<FluffyBunny.EntityFramework.Entities.PersistedGrantExtra,
                FluffyBunny.EntityFramework.Entities.PersistedGrantExtra>();
            cfg.CreateMap<FluffyBunny.EntityFramework.Entities.PersistedGrantExtra,
                FluffyBunny4.Models.PersistedGrantExtra>();
            cfg.CreateMap<FluffyBunny4.Models.PersistedGrantExtra,
                FluffyBunny.EntityFramework.Entities.PersistedGrantExtra>();
            cfg.CreateMap<FluffyBunny4.Models.PersistedGrantExtra, Duende.IdentityServer.Models.PersistedGrant>();
            cfg.CreateMap<Duende.IdentityServer.Models.PersistedGrant, FluffyBunny4.Models.PersistedGrantExtra>();
            cfg.CreateMap<Duende.IdentityServer.Models.PersistedGrant,
                FluffyBunny.EntityFramework.Entities.PersistedGrantExtra>();


            cfg.CreateMap<Duende.IdentityServer.EntityFramework.Entities.ApiResource,
                Duende.IdentityServer.EntityFramework.Entities.ApiResource>();
            cfg.CreateMap<Duende.IdentityServer.EntityFramework.Entities.ApiResource,
                    Duende.IdentityServer.Models.ApiResource>(MemberList.Destination)
                .ConstructUsing(src => new Duende.IdentityServer.Models.ApiResource())
                .ForMember(dest => dest.Scopes,
                    opt =>
                    {
                        opt.MapFrom(src => src.Scopes);
                        opt.ConvertUsing(new ApiResourceScopeConverter());
                    })
                .ForMember(x => x.ApiSecrets, opts => opts.MapFrom(x => x.Secrets));



        });
    }
}
