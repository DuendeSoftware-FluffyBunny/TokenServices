using Microsoft.Extensions.Logging;

namespace TokenService
{
    public class Global
    {
        public static IHostContext HostContext { get; set; }
        public static ILoggerProvider LoggerProvider { get; set; }
    }
}