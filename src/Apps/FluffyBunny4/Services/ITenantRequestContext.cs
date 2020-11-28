using System.Collections.Generic;

namespace FluffyBunny4.Services
{
    public interface ITenantRequestContext
    {
        string TenantId { get; set; }
        Dictionary<string,object> Storage { get; }
    }
}
