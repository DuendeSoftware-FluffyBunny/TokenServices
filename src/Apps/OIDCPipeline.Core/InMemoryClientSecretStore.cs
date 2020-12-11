
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OIDCPipeline.Core
{
    public class ClientRecord
    {
        public string Secret { get; set; }
        public List<string> RedirectUris { get; set; }
        public string ClientId { get; set; }
    }

    public class OIDCSchemeRecord
    {
        public List<ClientRecord> ClientRecords { get; set; }
        public List<string> AllowedProtocolParamaters { get; set; }
    }

    public class InMemoryClientSecretStore: IOIDCPipelineClientStore
    {
        private Dictionary<string, OIDCSchemeRecord> _records;

        public InMemoryClientSecretStore(Dictionary<string, OIDCSchemeRecord> records)
        {
            _records = records;
        }

        public Task<List<string>> FetchAllowedProtocolParamatersAsync(string scheme)
        {
            List<string> result = null;
            OIDCSchemeRecord record = null;
            if (_records.TryGetValue(scheme, out record))
            {
                result = record.AllowedProtocolParamaters;
            }
            
            return Task.FromResult(result??new List<string>());
        }

        public async Task<ClientRecord> FetchClientRecordAsync(string scheme,string clientId)
        {
            var record = (from item in _records
                from r in item.Value.ClientRecords
                where r.ClientId == clientId
                select r).FirstOrDefault();
            return record;
        }

    

        public async Task<string> FetchClientSecretAsync(string scheme, string clientId)
        {
            string result = null;
            var record = await FetchClientRecordAsync(scheme, clientId);
            return record.Secret;
        }
    }
}
