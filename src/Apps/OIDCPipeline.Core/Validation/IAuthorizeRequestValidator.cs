using OIDCPipeline.Core.Validation.Models;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace OIDCPipeline.Core.Validation
{
    /// <summary>
    ///  Authorize endpoint request validator.
    /// </summary>
    internal interface IAuthorizeRequestValidator
    {
        /// <summary>
        ///  Validates authorize request parameters.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        Task<AuthorizeRequestValidationResult> ValidateAsync(ValidatedAuthorizeRequest request);
      
    }
}
