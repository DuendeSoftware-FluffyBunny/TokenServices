using AutoMapper;

namespace FluffyBunny4.DotNetCore.Services
{
    public interface IMapperAccessor
    {
        IMapper Mapper { get; }
    }
}