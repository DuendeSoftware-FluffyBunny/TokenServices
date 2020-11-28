using FluffyBunny4.DotNetCore;
using FluffyBunny4.Services;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FluffyBunny4.Services
{
    internal class TenantRequestContext : ITenantRequestContext
    {
        public string TenantId { get; set; }
        public Dictionary<string, object> _storage;

        public Dictionary<string, object> Storage
        {
            get
            {
                if (_storage == null)
                {
                    _storage = new Dictionary<string, object>();
                }

                return _storage;
            }
        }
    }
}
