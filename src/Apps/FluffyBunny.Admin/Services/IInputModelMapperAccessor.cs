using AutoMapper;

namespace FluffyBunny.Admin.Services
{
    public interface IInputModelMapperAccessor
    {
        IMapper MapperOneToOne { get; }
        IMapper MapperIgnoreBase { get; set; }
    }
}