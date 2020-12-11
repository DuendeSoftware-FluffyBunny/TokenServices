using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace OIDCConsentOrchestrator.Models.Client
{

    /// <summary>
    /// Models a base  Request
    /// </summary>
    public class ConsentProtocolRequest : HttpRequestMessage
    {
        /// <summary>
        /// Initializes an the HTTP protocol request and sets the accept header to application/json
        /// </summary>
        public ConsentProtocolRequest()
        {
            Headers.Accept.Clear();
            Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Gets or sets the endpoint address (you can also set the RequestUri instead or leave blank to use the HttpClient base address).
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public string Address { get; set; }


        /// <summary>
        /// Clones this instance.
        /// </summary>
        public ConsentProtocolRequest Clone()
        {
            return Clone<ConsentProtocolRequest>();
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        public T Clone<T>()
            where T : ConsentProtocolRequest, new()
        {
            var clone = new T
            {
                RequestUri = RequestUri,
                Version = Version,
                Method = Method,
                Address = Address
               
            };
            clone.Headers.Clear();
            foreach (var header in Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }

        /// <summary>
        /// Applies protocol parameters to HTTP request
        /// </summary>
        public void Prepare()
        {
            
 
        }
    }
}
