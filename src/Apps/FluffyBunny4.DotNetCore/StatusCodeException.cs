using System;
using System.Net;

namespace FluffyBunny4.DotNetCore
{
    public class StatusCodeException : Exception
    {
        public StatusCodeException(HttpStatusCode statusCode, string message = null, Exception innerException = null)
            : base(message??$"StatusCode:{statusCode}", innerException)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
