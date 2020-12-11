using FluffyBunny4.DotNetCore;
using OIDCPipeline.Core.Validation.Models;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace OIDCPipeline.Core.Validation
{
    public interface ITokenRequestValidator
    {
        /// <summary>
        /// Validates the request.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        Task<TokenRequestValidationResult> ValidateRequestAsync(SimpleNameValueCollection parameters);
    }
}
