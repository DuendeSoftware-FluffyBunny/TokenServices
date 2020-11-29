using AutoMapper;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Services
{
    public interface IEntityFrameworkMapperAccessor
    {
        IMapper MapperOneToOne { get; }
        IMapper MapperIgnoreBase { get; set; }
    }
}