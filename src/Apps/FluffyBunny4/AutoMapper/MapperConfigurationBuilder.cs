using AutoMapper;
using Duende.IdentityServer.Models;
using FluffyBunny4.DotNetCore.Extensions;
using FluffyBunny4.Models;

namespace FluffyBunny4.AutoMapper
{
    public static class MapperConfigurationBuilder
    {

        public static IMapper BuidOneToOneMapper => BuidOneToOneMapperConfiguration.CreateMapper();

        public static MapperConfiguration BuidOneToOneMapperConfiguration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<PersistedGrant, PersistedGrantExtra>();
            cfg.CreateMap<PersistedGrantExtra, PersistedGrant>();

            cfg.CreateMap<ClientExtra, ClientExtraHandle>();
            cfg.CreateMap<ClientExtraHandle, ClientExtra>();

            cfg.CreateMap<ApiScope, ApiScopeHandle>();
            cfg.CreateMap<ApiScopeHandle, ApiScope>();

            cfg.CreateMap<ApiScope, TenantApiScopeHandle>();
            cfg.CreateMap<TenantApiScopeHandle, ApiScope>();

            cfg.CreateMap<ApiResource, ApiResourceHandle>();
            cfg.CreateMap<ApiResourceHandle, ApiResource>();

            cfg.CreateMap<Secret, SecretHandle>();
            cfg.CreateMap<SecretHandle, Secret>();


            cfg.CreateMap<ApiResource, TenantApiResourceHandle>();
            cfg.CreateMap<TenantApiResourceHandle, ApiResource>();

            cfg.CreateMap<DeviceCode, DeviceCodeExtra>()
                .ForMember(dest => dest.AuthorizedScopes, opt => opt.ConvertUsing(new AuthorizedScopesConverter()));
        });
    }
}
