using Newtonsoft.Json;
using System.Collections.Generic;

namespace FluffyBunny4.Models
{
    public class ApiResourceHandle : ResouceHandle
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        private List<SecretHandle> _apiSecrets;
        public List<SecretHandle> ApiSecrets
        {
            get
            {
                if (_apiSecrets == null)
                {
                    _apiSecrets = new List<SecretHandle>();
                }
                return _apiSecrets;
            }
         
            set { _apiSecrets = value; }
        }
        private List<string> _scopes;
        public List<string> Scopes
        {
            get
            {
                if (_scopes == null)
                {
                    _scopes = new List<string>();
                }
                return _scopes;
            }

            set { _scopes = value; }
        }
        private List<string> _allowedAccessTokenSigningAlgorithms;
        public List<string> AllowedAccessTokenSigningAlgorithms
        {
            get
            {
                if (_allowedAccessTokenSigningAlgorithms == null)
                {
                    _allowedAccessTokenSigningAlgorithms = new List<string>();
                }
                return _allowedAccessTokenSigningAlgorithms;
            }

            set { _allowedAccessTokenSigningAlgorithms = value; }
        }
    }
}
