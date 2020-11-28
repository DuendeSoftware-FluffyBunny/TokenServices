using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluffyBunny4.Services
{
    public interface IMapperAccessor
    {
        IMapper Mapper { get; }
    }
}
