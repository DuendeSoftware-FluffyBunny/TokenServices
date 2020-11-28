using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluffyBunny4.Services.Default
{
    internal class CoreMapperAccessor : ICoreMapperAccessor
    {
        public IMapper Mapper { get; }

        public CoreMapperAccessor(IMapper mapper)
        {
            Mapper = mapper;
        }
    }
}
