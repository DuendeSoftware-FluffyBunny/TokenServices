
using Microsoft.Extensions.Logging;
using OIDCPipeline.Core.Configuration;
using OIDCPipeline.Core.Extensions;
using OIDCPipeline.Core.Logging.Models;
using OIDCPipeline.Core.Validation.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Text;
using IdentityModel;
using OIDCPipeline.Core.Endpoints.ResponseHandling;
using FluffyBunny4.DotNetCore;
using Microsoft.AspNetCore.Mvc;

namespace OIDCPipeline.Core.Validation
{
    internal class DefaultTokenRequestValidator : ITokenRequestValidator
    {
        private OIDCPipelineOptions _options;
        private IOIDCPipelineClientStore _clientSecretStore;
        private IOIDCPipelineStore _oidcPipelineStore;
        private ILogger<DefaultTokenRequestValidator> _logger;
        private ValidatedTokenRequest _validatedRequest;

        public DefaultTokenRequestValidator(
            OIDCPipelineOptions options,
            IOIDCPipelineClientStore clientSecretStore,
            IOIDCPipelineStore oidcPipelineStore,
            ILogger<DefaultTokenRequestValidator> logger)
        {
            _options = options;
            _clientSecretStore = clientSecretStore;
            _oidcPipelineStore = oidcPipelineStore;
            _logger = logger;
        }
        public async Task<TokenRequestValidationResult> ValidateRequestAsync(SimpleNameValueCollection parameters)
        {
            _logger.LogDebug("Start token request validation");
            _validatedRequest = new ValidatedTokenRequest
            {
                Raw = parameters ?? throw new ArgumentNullException(nameof(parameters))
            };
            /////////////////////////////////////////////
            // check grant type
            /////////////////////////////////////////////
            var clientId = parameters.Get(OidcConstants.TokenRequest.ClientId);
            if (clientId.IsMissing())
            {
                LogError("ClientId is missing");
                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
            }
            _validatedRequest.ClientId = clientId;
            /////////////////////////////////////////////
            // check grant type
            /////////////////////////////////////////////
            var grantType = parameters.Get(OidcConstants.TokenRequest.GrantType);
            if (grantType.IsMissing())
            {
                LogError("Grant type is missing");
                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
            }

            if (grantType.Length > _options.InputLengthRestrictions.GrantType)
            {
                LogError("Grant type is too long");
                return Invalid(OidcConstants.TokenErrors.UnsupportedGrantType);
            }

            _validatedRequest.GrantType = grantType;
            switch (grantType)
            {
                case OidcConstants.GrantTypes.AuthorizationCode:
                    return await RunValidationAsync(ValidateAuthorizationCodeRequestAsync, parameters);
                 
                default:
                    return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }
 
        }
        private async Task<TokenRequestValidationResult> RunValidationAsync(Func<SimpleNameValueCollection, Task<TokenRequestValidationResult>> validationFunc, SimpleNameValueCollection parameters)
        {
            // run standard validation
            var result = await validationFunc(parameters);
            if (!result.IsError)
            {
                LogSuccess();
            }
            return result;
        }

        private async Task<TokenRequestValidationResult> ValidateAuthorizationCodeRequestAsync(SimpleNameValueCollection parameters)
        {
            _logger.LogDebug("Start validation of authorization code token request");

            /////////////////////////////////////////////
            // validate authorization code
            /////////////////////////////////////////////
            var code = parameters.Get(OidcConstants.TokenRequest.Code);
            if (code.IsMissing())
            {
                LogError("Authorization code is missing");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            if (code.Length > _options.InputLengthRestrictions.AuthorizationCode)
            {
                LogError("Authorization code is too long");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            _validatedRequest.AuthorizationCodeHandle = code;
            _validatedRequest.IdTokenResponse = await _oidcPipelineStore.GetDownstreamIdTokenResponseAsync(code);
             
            if (_validatedRequest.IdTokenResponse == null)
            {
                LogError("Invalid authorization code", new { code });
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }
            await _oidcPipelineStore.DeleteStoredCacheAsync(code);

           
            /////////////////////////////////////////////
            // validate client binding
            /////////////////////////////////////////////
            if (_validatedRequest.IdTokenResponse.Request.ClientId != _validatedRequest.ClientId)
            {
                LogError("Client is trying to use a code from a different client", 
                    new { clientId = _validatedRequest.ClientId, codeClient = _validatedRequest.IdTokenResponse.Request.ClientId });
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            /////////////////////////////////////////////
            // validate redirect_uri
            /////////////////////////////////////////////
            var redirectUri = parameters.Get(OidcConstants.TokenRequest.RedirectUri);
            if (redirectUri.IsMissing())
            {
                LogError("Redirect URI is missing");
                return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
            }
           
            if (redirectUri.Equals(_validatedRequest.IdTokenResponse.Request.RedirectUri, StringComparison.Ordinal) == false)
            {
                LogError("Invalid redirect_uri", new { redirectUri, expectedRedirectUri = _validatedRequest.IdTokenResponse.Request.RedirectUri });
                return Invalid(OidcConstants.TokenErrors.UnauthorizedClient);
            }


            /////////////////////////////////////////////
            // validate PKCE parameters
            /////////////////////////////////////////////
            var codeVerifier = parameters.Get(OidcConstants.TokenRequest.CodeVerifier);
            if (_validatedRequest.IdTokenResponse.Request.CodeChallenge.IsPresent())
            {
                _logger.LogDebug("Client required a proof key for code exchange. Starting PKCE validation");

                var proofKeyResult = ValidateAuthorizationCodeWithProofKeyParameters(codeVerifier, _validatedRequest.IdTokenResponse);
                if (proofKeyResult.IsError)
                {
                    return proofKeyResult;
                }

                _validatedRequest.CodeVerifier = codeVerifier;
            }
            else
            {
                if (codeVerifier.IsPresent())
                {
                    LogError("Unexpected code_verifier: {codeVerifier}. This happens when the client is trying to use PKCE, but it is not enabled. Set RequirePkce to true.", codeVerifier);
                    return Invalid(OidcConstants.TokenErrors.InvalidGrant);
                }
            }

            _logger.LogDebug("Validation of authorization code token request success");

            return Valid();
        }
        private TokenRequestValidationResult ValidateAuthorizationCodeWithProofKeyParameters(string codeVerifier, DownstreamAuthorizeResponse idTokenResopnse)
        {
            if (idTokenResopnse.Request.CodeChallenge.IsMissing() || idTokenResopnse.Request.CodeChallengeMethod.IsMissing())
            {
                LogError("Client is missing code challenge or code challenge method", new { clientId = _validatedRequest.ClientId });
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            if (codeVerifier.IsMissing())
            {
                LogError("Missing code_verifier");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            if (codeVerifier.Length < _options.InputLengthRestrictions.CodeVerifierMinLength ||
                codeVerifier.Length > _options.InputLengthRestrictions.CodeVerifierMaxLength)
            {
                LogError("code_verifier is too short or too long");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            if (Constants.SupportedCodeChallengeMethods.Contains(idTokenResopnse.Request.CodeChallengeMethod) == false)
            {
                LogError("Unsupported code challenge method", new { codeChallengeMethod = idTokenResopnse.Request.CodeChallengeMethod });
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            if (ValidateCodeVerifierAgainstCodeChallenge(codeVerifier, idTokenResopnse.Request.CodeChallenge, idTokenResopnse.Request.CodeChallengeMethod) == false)
            {
                LogError("Transformed code verifier does not match code challenge");
                return Invalid(OidcConstants.TokenErrors.InvalidGrant);
            }

            return Valid();
        }

        private bool ValidateCodeVerifierAgainstCodeChallenge(string codeVerifier, string codeChallenge, string codeChallengeMethod)
        {
            if (codeChallengeMethod == OidcConstants.CodeChallengeMethods.Plain)
            {
                return TimeConstantComparer.IsEqual(codeVerifier.Sha256(), codeChallenge);
            }

            var codeVerifierBytes = Encoding.ASCII.GetBytes(codeVerifier);
            var hashedBytes = codeVerifierBytes.Sha256();
            var transformedCodeVerifier = Base64Url.Encode(hashedBytes);

            return TimeConstantComparer.IsEqual(transformedCodeVerifier, codeChallenge);
        }

        private TokenRequestValidationResult Valid(Dictionary<string, object> customResponse = null)
        {
            return new TokenRequestValidationResult(_validatedRequest, customResponse);
        }

        private TokenRequestValidationResult Invalid(string error, string errorDescription = null, Dictionary<string, object> customResponse = null)
        {
            return new TokenRequestValidationResult(_validatedRequest, error, errorDescription, customResponse);
        }
        private void LogSuccess()
        {
            var details = new TokenRequestValidationLog(_validatedRequest);
            _logger.LogInformation("Token request validation success\n{@details}", details);
        }

        private void LogError(string message = null, object values = null)
        {
            var details = new TokenRequestValidationLog(_validatedRequest);

            if (message.IsPresent())
            {
                try
                {
                    if (values == null)
                    {
                        _logger.LogError(message + ", {@details}", details);
                    }
                    else
                    {
                        _logger.LogError(message + "{@values}, details: {@details}", values, details);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError("Error logging {exception}, request details: {@details}", ex.Message, details);
                }
            }
            else
            {
                _logger.LogError("{@details}", details);
            }
        }
    }
}
