namespace OIDCPipeline.Core.Validation.Models
{
    /// <summary>
    /// Validation result for authorize requests
    /// </summary>
    internal class AuthorizeRequestValidationResult : ValidationResult
    {
        public ValidatedAuthorizeRequest ValidatedAuthorizeRequest { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeRequestValidationResult"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        public AuthorizeRequestValidationResult(ValidatedAuthorizeRequest request)
        {
            ValidatedAuthorizeRequest = request;
            IsError = false;
            ErrorDescription = null;
            Error = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizeRequestValidationResult" /> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="error">The error.</param>
        /// <param name="errorDescription">The error description.</param>
        public AuthorizeRequestValidationResult(ValidatedAuthorizeRequest request,string error, string errorDescription = null)
        {
            ValidatedAuthorizeRequest = request;
            IsError = true;
            Error = error;
            ErrorDescription = errorDescription;
        }
    }
}
