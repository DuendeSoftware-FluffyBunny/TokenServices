using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace FluffyBunny4.Models
{
     
    public class TenantHandle
    {
        public string _id;
        [JsonProperty("id")]
        public string Id
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_id))
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(TenantId));
                        Guid result = new Guid(hash);
                        _id = result.ToString();
                    }
                }
                return _id;
            }
             
        }
        [JsonProperty("tenantId")] 
        public string TenantId { get; set; }
        string _name;
        [JsonProperty("name")]
        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_name))
                {
                    _name = $"{TenantId} Tenant";
                }
                return _name;
            }
            set { _name = value; }
        }
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = false;
        [JsonProperty("properties")]
        public Dictionary<string,string> Properties { get; set; }
    }
}
