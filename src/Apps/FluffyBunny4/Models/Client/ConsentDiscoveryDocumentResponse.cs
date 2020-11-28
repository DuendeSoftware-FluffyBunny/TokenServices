using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using FluffyBunny4.DotNetCore.Client;

namespace FluffyBunny4.Models.Client
{
    public class ConsentDiscoveryDocumentResponse : ConsentProtocolResponse
    {

        public string AuthorizeEndpoint => TryGetString(Constants.Discovery.AuthorizationEndpoint);
        public List<string> ScopesSupported => TryGetStringArray(Constants.Discovery.ScopesSupported).ToList();
        public string AuthorizationType => TryGetString(Constants.Discovery.AuthorizationType);

        // generic
        public JToken TryGetValue(string name) => Json.TryGetValue(name);
        public string TryGetString(string name) => Json.TryGetString(name);
        public bool? TryGetBoolean(string name) => Json.TryGetBoolean(name);
        public IEnumerable<string> TryGetStringArray(string name) => Json.TryGetStringArray(name);
    }
}
