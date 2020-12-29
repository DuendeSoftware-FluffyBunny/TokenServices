using AutoMapper;
using Duende.IdentityServer.EntityFramework.Entities;
using FluffyBunny.Admin.Pages.Tenants.Tenant.Clients.Models;
using FluffyBunny.EntityFramework.Entities;
using FluffyBunny4.DotNetCore.Extensions;

namespace FluffyBunny.Admin.AutoMapper
{
    public static class MapperConfigurationBuilder
    {
        public static IMapper BuidIgnoreBaseMapper => BuidIgnoreBaseMapperConfiguration.CreateMapper();
        public static MapperConfiguration BuidIgnoreBaseMapperConfiguration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ClientExtra, ClientExtraInputModel>()
                .Ignore(record => record.Id);
            cfg.CreateMap<ClientExtraInputModel, ClientExtra>()
                .Ignore(record => record.Id);
        });

        public static IMapper BuidOneToOneMapper => BuidOneToOneMapperConfiguration.CreateMapper();
        public static MapperConfiguration BuidOneToOneMapperConfiguration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ClientExtra, ClientExtraInputModel>();
            cfg.CreateMap<ClientExtraInputModel, ClientExtra>();
        });
    }
}
