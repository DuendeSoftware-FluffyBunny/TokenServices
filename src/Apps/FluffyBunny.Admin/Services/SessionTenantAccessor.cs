using FluffyBunny.IdentityServer.EntityFramework.Storage.Services;
using Microsoft.AspNetCore.Http;

namespace FluffyBunny.Admin.Services
{
    public class SessionTenantAccessor: ISessionTenantAccessor
    {
        private const string Key = "13698e83-06b3-4e26-853a-144cbd6d8c81";
        private IHttpContextAccessor _httpContextAccessor;

        public SessionTenantAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string TenantId
        {
            get
            {
                var tenantId = _httpContextAccessor.HttpContext.Session.GetString(Key);
                return tenantId;
            }
            set
            {
                _httpContextAccessor.HttpContext.Session.SetString(Key, value);
            }
        }
    }
}
