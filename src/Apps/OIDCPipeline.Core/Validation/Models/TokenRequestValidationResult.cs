using System;
using System.Collections.Generic;
using System.Text;

namespace OIDCPipeline.Core.Validation.Models
{
    public class TokenRequestValidationResult : ValidationResult
    {
        public ValidatedTokenRequest Request;
        public Dictionary<string, object> CustomResponse;

        public TokenRequestValidationResult(ValidatedTokenRequest validatedRequest, Dictionary<string, object> customResponse)
        {
            IsError = false;
            Request = validatedRequest;
            CustomResponse = customResponse;
        }

        public TokenRequestValidationResult(ValidatedTokenRequest validatedRequest, string error, string errorDescription, Dictionary<string, object> customResponse)
        {
            IsError = true;
            Request = validatedRequest;
            Error = error;
            ErrorDescription = errorDescription;
            CustomResponse = customResponse;
        }
    }
}
