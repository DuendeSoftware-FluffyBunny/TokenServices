using OpenIdConnectModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OIDCPipeline.Core.Services
{
    public interface IOpenIdConnectSchemeRecords
    {
        Task<OpenIdConnectSchemeRecord> GetOpenIdConnectSchemeRecordBySchemeAsync(string scheme);
    }
    public class InMemoryOpenIdConnectSchemeRecords: IOpenIdConnectSchemeRecords
    {
        private List<OpenIdConnectSchemeRecord> _openIdConnectSchemeRecords;

        public InMemoryOpenIdConnectSchemeRecords(List<OpenIdConnectSchemeRecord> openIdConnectSchemeRecords)
        {
            _openIdConnectSchemeRecords = openIdConnectSchemeRecords;
        }

        public async Task<OpenIdConnectSchemeRecord> GetOpenIdConnectSchemeRecordBySchemeAsync(string scheme)
        {
            var result = (from item in _openIdConnectSchemeRecords
                          where item.Scheme == scheme
                          select item).FirstOrDefault();
            return result;
        }
    }
}
