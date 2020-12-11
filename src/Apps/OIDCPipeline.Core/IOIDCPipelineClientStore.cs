using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public interface IOIDCPipelineClientStore
    {
       
        Task<List<string>> FetchAllowedProtocolParamatersAsync(string scheme);
        Task<ClientRecord> FetchClientRecordAsync(string scheme, string clientId);
    }
}
