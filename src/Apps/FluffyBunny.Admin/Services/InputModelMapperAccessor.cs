using AutoMapper;

namespace FluffyBunny.Admin.Services
{
    internal class InputModelMapperAccessor : IInputModelMapperAccessor
    {
        public IMapper MapperOneToOne { get; set; }
        public IMapper MapperIgnoreBase { get; set; }
    }
}