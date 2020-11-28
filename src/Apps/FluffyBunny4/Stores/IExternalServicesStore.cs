using FluffyBunny4.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluffyBunny4.Stores
{
    public interface IExternalServicesStore
    {
        Task<List<ExternalService>> GetExternalServicesAsync();
        Task<ExternalService> GetExternalServiceByNameAsync(string serviceName);
    }
}
