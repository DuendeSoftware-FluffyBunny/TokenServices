using Microsoft.AspNetCore.Identity;

namespace FluffyBunny.Admin.Services
{
    public interface IExternalLoginContext
    {
        public ExternalLoginInfo ExternalLoginInfo { get; set; }
    }

    public class ExternalLoginContext : IExternalLoginContext
    {
        public ExternalLoginInfo ExternalLoginInfo { get; set; }
    }
}