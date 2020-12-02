using AutoMapper;
using FluffyBunny4.Azure.Services;

namespace FluffyBunny4.Services
{
    internal class CosmosMapperAccessor : ICosmosMapperAccessor
    {
        public IMapper Mapper { get; }

        public CosmosMapperAccessor(IMapper mapper)
        {
            Mapper = mapper;
        }
    }
}
