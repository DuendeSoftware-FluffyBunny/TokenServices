using IdentityModel;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OIDCPipeline.Core.Configuration;
using OIDCPipeline.Core.Extensions;
using OIDCPipeline.Core.Logging.Models;
using OIDCPipeline.Core.Validation.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace OIDCPipeline.Core.Validation.Default
{
    internal class DefaultAuthorizeRequestValidator : IAuthorizeRequestValidator
    {
        private OIDCPipelineOptions _options;
        private IOIDCPipelineClientStore _clientSecretStore;
        private ILogger<DefaultAuthorizeRequestValidator> _logger;

        public DefaultAuthorizeRequestValidator(
            OIDCPipelineOptions options,
            IOIDCPipelineClientStore clientSecretStore,
            ILogger<DefaultAuthorizeRequestValidator> logger)
        {
            _options = options;
            _clientSecretStore = clientSecretStore;
            _logger = logger;
        }
        private readonly ResponseTypeEqualityComparer
          _responseTypeEqualityComparer = new ResponseTypeEqualityComparer();
        public static readonly List<string> SupportedResponseTypes = new List<string>
        {
            OidcConstants.ResponseTypes.IdToken,
            OidcConstants.ResponseTypes.IdTokenToken,
            OidcConstants.ResponseTypes.Code
        };
        public static readonly List<string> SupportedResponseModes = new List<string>
        {
            OidcConstants.ResponseModes.FormPost,
            OidcConstants.ResponseModes.Query,
            OidcConstants.ResponseModes.Fragment
        };

        public async Task<AuthorizeRequestValidationResult> ValidateAsync(ValidatedAuthorizeRequest request)
        {
            var parameters = request.Raw;
            request.Nonce = parameters.Get(OidcConstants.AuthorizeRequest.Nonce);

            var state = parameters.Get(OidcConstants.AuthorizeRequest.State);
            if (state.IsPresent())
            {
                request.State = state;
            }

            //////////////////////////////////////////////////////////
            // redirect_uri must be present and supported
            //////////////////////////////////////////////////////////
            var redirectUri = parameters.Get(OidcConstants.AuthorizeRequest.RedirectUri);
            if (redirectUri.IsMissing())
            {
                _logger.LogError($"Missing {OidcConstants.AuthorizeRequest.RedirectUri}");
                return Invalid(request, OidcConstants.AuthorizeErrors.InvalidRequestUri, $"Missing {OidcConstants.AuthorizeRequest.RedirectUri}");
            }

            //////////////////////////////////////////////////////////
            // response_type must be present and supported
            //////////////////////////////////////////////////////////
            var responseType = parameters.Get(OidcConstants.AuthorizeRequest.ResponseType);
            if (responseType.IsMissing())
            {
                _logger.LogError($"Missing {OidcConstants.AuthorizeRequest.ResponseType}");
                return Invalid(request, OidcConstants.AuthorizeErrors.UnsupportedResponseType, $"Missing {OidcConstants.AuthorizeRequest.ResponseType}");
            }
            if (!SupportedResponseTypes.Contains(responseType, _responseTypeEqualityComparer))
            {
                _logger.LogError("Response type not supported", responseType);
                return Invalid(request, OidcConstants.AuthorizeErrors.UnsupportedResponseType, "Response type not supported");
            }
            request.ResponseType = SupportedResponseTypes.First(
              supportedResponseType => _responseTypeEqualityComparer.Equals(supportedResponseType, responseType));

            //////////////////////////////////////////////////////////
            // match response_type to grant type
            //////////////////////////////////////////////////////////
            request.GrantType = Constants.ResponseTypeToGrantTypeMapping[responseType];
            //////////////////////////////////////////////////////////
            // check if flow is allowed at authorize endpoint
            //////////////////////////////////////////////////////////
            if (!Constants.AllowedGrantTypesForAuthorizeEndpoint.Contains(request.GrantType))
            {
                LogError("Invalid grant type", request.GrantType, request);
                return Invalid(request, description: "Invalid response_type");
            }

            // set default response mode for flow; this is needed for any client error processing below
            request.ResponseMode = Constants.AllowedResponseModesForGrantType[request.GrantType].First();

            //////////////////////////////////////////////////////////
            // client_id must be present and supported
            //////////////////////////////////////////////////////////

            request.ClientId = parameters.Get(OidcConstants.AuthorizeRequest.ClientId);


            if (request.ClientId.IsMissing())
            {
                _logger.LogError($"Missing {OidcConstants.AuthorizeRequest.ClientId}");
                return Invalid(request, OidcConstants.AuthorizeErrors.UnauthorizedClient, $"Missing {OidcConstants.AuthorizeRequest.ClientId}");
            }
            //////////////////////////////////////////////////////////
            // redirect_uri must be present, and a valid uri
            //////////////////////////////////////////////////////////
            request.RedirectUri = parameters.Get(OidcConstants.AuthorizeRequest.RedirectUri);

            if (request.RedirectUri.IsMissing())
            {
                _logger.LogError($"Missing {OidcConstants.AuthorizeRequest.RedirectUri}");
                return Invalid(request, OidcConstants.AuthorizeRequest.RedirectUri, $"Missing {OidcConstants.AuthorizeRequest.RedirectUri}");
            }

            var clientRecord = await _clientSecretStore.FetchClientRecordAsync(_options.Scheme, request.ClientId);
            if (clientRecord == null)
            {
                _logger.LogError($"Missing {OidcConstants.AuthorizeRequest.ClientId}");
                return Invalid(request, OidcConstants.AuthorizeErrors.UnauthorizedClient, $"Missing {OidcConstants.AuthorizeRequest.ClientId}");
            }
            if (!clientRecord.RedirectUris.Contains(request.RedirectUri))
            {
                _logger.LogError($"Missing {OidcConstants.AuthorizeRequest.RedirectUri}");
                return Invalid(request, OidcConstants.AuthorizeErrors.UnauthorizedClient, $"Missing {OidcConstants.AuthorizeRequest.ClientId}");
            }

            //////////////////////////////////////////////////////////
            // check response_mode parameter and set response_mode
            //////////////////////////////////////////////////////////

            // check if response_mode parameter is present and valid
            var responseMode = parameters.Get(OidcConstants.AuthorizeRequest.ResponseMode);
            if (responseMode.IsPresent())
            {
                if (Constants.SupportedResponseModes.Contains(responseMode))
                {
                    if (Constants.AllowedResponseModesForGrantType[request.GrantType].Contains(responseMode))
                    {
                        request.ResponseMode = responseMode;
                    }
                    else
                    {
                        LogError("Invalid response_mode for flow", responseMode, request);
                        return Invalid(request, OidcConstants.AuthorizeErrors.UnsupportedResponseType, description: "Invalid response_mode");
                    }
                }
                else
                {
                    LogError("Unsupported response_mode", responseMode, request);
                    return Invalid(request, OidcConstants.AuthorizeErrors.UnsupportedResponseType, description: "Invalid response_mode");
                }
            }
            //////////////////////////////////////////////////////////
            // check if PKCE is required and validate parameters
            //////////////////////////////////////////////////////////
            if (request.GrantType == GrantType.AuthorizationCode || request.GrantType == GrantType.Hybrid)
            {
                _logger.LogDebug("Checking for PKCE parameters");

                /////////////////////////////////////////////////////////////////////////////
                // validate code_challenge and code_challenge_method
                /////////////////////////////////////////////////////////////////////////////
                var proofKeyResult = ValidatePkceParameters(request);

                if (proofKeyResult.IsError)
                {
                    return proofKeyResult;
                }
            }

            return Valid(request);
        }

        private AuthorizeRequestValidationResult ValidatePkceParameters(ValidatedAuthorizeRequest request)
        {
            var fail = Invalid(request);

            var codeChallenge = request.Raw.Get(OidcConstants.AuthorizeRequest.CodeChallenge);
            if (codeChallenge.IsMissing())
            {
                return Valid(request);
            }

            if (codeChallenge.Length < _options.InputLengthRestrictions.CodeChallengeMinLength ||
                codeChallenge.Length > _options.InputLengthRestrictions.CodeChallengeMaxLength)
            {
                LogError("code_challenge is either too short or too long", request);
                fail.ErrorDescription = "Invalid code_challenge";
                return fail;
            }

            request.CodeChallenge = codeChallenge;

            var codeChallengeMethod = request.Raw.Get(OidcConstants.AuthorizeRequest.CodeChallengeMethod);
            if (codeChallengeMethod.IsMissing())
            {
                _logger.LogDebug("Missing code_challenge_method, defaulting to plain");
                codeChallengeMethod = OidcConstants.CodeChallengeMethods.Plain;
            }

            if (!Constants.SupportedCodeChallengeMethods.Contains(codeChallengeMethod))
            {
                LogError("Unsupported code_challenge_method", codeChallengeMethod, request);
                fail.ErrorDescription = "Transform algorithm not supported";
                return fail;
            }

            // check if plain method is allowed
            if (codeChallengeMethod == OidcConstants.CodeChallengeMethods.Plain)
            {
                if (!_options.AllowPlainTextPkce)
                {
                    LogError("code_challenge_method of plain is not allowed", request);
                    fail.ErrorDescription = "Transform algorithm not supported";
                    return fail;
                }
            }

            request.CodeChallengeMethod = codeChallengeMethod;

            return Valid(request);
        }
        private AuthorizeRequestValidationResult Valid(ValidatedAuthorizeRequest request)
        {
            return new AuthorizeRequestValidationResult(request);
        }
        private AuthorizeRequestValidationResult Invalid(ValidatedAuthorizeRequest request,
             string error = OidcConstants.AuthorizeErrors.InvalidRequest, string description = null)
        {
            return new AuthorizeRequestValidationResult(request, error, description);
        }
        private void LogError(string message, ValidatedAuthorizeRequest request)
        {
            var requestDetails = new AuthorizeRequestValidationLog(request);
            _logger.LogError(message + "\n{@requestDetails}", requestDetails);
        }
        private void LogError(string message, string detail, ValidatedAuthorizeRequest request)
        {
            var requestDetails = new AuthorizeRequestValidationLog(request);
            _logger.LogError(message + ": {detail}\n{@requestDetails}", detail, requestDetails);
        }
    }
}
