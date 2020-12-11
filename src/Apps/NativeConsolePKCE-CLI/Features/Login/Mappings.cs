using AutoMapper;

namespace NativeConsolePKCE_CLI.Features.Login
{
    public class Mappings : Profile
    {
        public Mappings()
        {
            CreateMap<Commands.LoginCommand, LoginInfo.Request>();
        }
    }
}
