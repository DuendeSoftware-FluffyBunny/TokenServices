using AutoMapper;

namespace FluffyBunny.IdentityServer.EntityFramework.Storage.Services
{
    internal class EntityFrameworkMapperAccessor :
        IEntityFrameworkMapperAccessor
    {
        public IMapper MapperOneToOne { get; set; }
        public IMapper MapperIgnoreBase { get; set; }
    }
}