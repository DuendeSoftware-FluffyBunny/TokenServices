using FluffyBunny4.DotNetCore.Collections;
using FluffyBunny4.Models;
using FluffyBunny4.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Duende.IdentityServer.ResponseHandling;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;

namespace FluffyBunny4.ResponseHandling
{
    /// <summary>
    /// Default revocation response generator
    /// </summary>
    /// <seealso cref="ITokenRevocationResponseGenerator" />
    public class TokenRevocationResponseGenerator : ITokenRevocationResponseGenerator
    {
        private IBackgroundTaskQueue<Delete> _taskQueueDelete;
        private readonly IPersistedGrantStore _persistedGrantStore;
        private readonly ITokenRevocationResponseGenerator _idsTokenRevocationResponseGenerator;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenRevocationResponseGenerator" /> class.
        /// </summary>
        /// <param name="idsTokenRevocationResponseGenerator">The original IdentityServer4 TokenRevocationResponseGenerator.</param>
        /// <param name="logger">The logger.</param>
        public TokenRevocationResponseGenerator(
            IPersistedGrantStore persistedGrantStore,
            IBackgroundTaskQueue<Delete> taskQueueDelete,
            Duende.IdentityServer.ResponseHandling.TokenRevocationResponseGenerator idsTokenRevocationResponseGenerator,
            ILogger<TokenRevocationResponseGenerator> logger)
        {
            _persistedGrantStore = persistedGrantStore;
            _taskQueueDelete = taskQueueDelete;
            _idsTokenRevocationResponseGenerator = idsTokenRevocationResponseGenerator;
            Logger = logger;
        }

        /// <summary>
        /// Creates the revocation endpoint response and processes the revocation request.
        /// </summary>
        /// <param name="validationResult">The userinfo request validation result.</param>
        /// <returns></returns>
        public virtual async Task<TokenRevocationResponse> ProcessAsync(TokenRevocationRequestValidationResult validationResult)
        {
            if (validationResult.TokenTypeHint == Constants.TokenTypeHints.Subject)
            {
                var response = new TokenRevocationResponse
                {
                    Success = false,
                    TokenType = validationResult.TokenTypeHint
                };
                var client = validationResult.Client as ClientExtra;
                if (client.AllowGlobalSubjectRevocation)
                {
                    Logger.LogInformation($"TokenRevocation: client={client.ClientId},subject={validationResult.Token}");
                    response.Success = await RevokeSubjectAsync(validationResult);
                }
                else
                {
                    Logger.LogError($"client={client.ClientId},AllowGlobalSubjectRevocation={client.AllowGlobalSubjectRevocation}");
                }
                return response;
            }
            else
            {
                return await _idsTokenRevocationResponseGenerator.ProcessAsync(validationResult);
            }
        }

        /// <summary>
        /// Revoke Every token associated with this subject.
        /// </summary>
        protected virtual async Task<bool> RevokeSubjectAsync(TokenRevocationRequestValidationResult validationResult)
        {
            try
            {
                DeferRemoveSubjectTokensAsync(validationResult.Token);
                return true;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex.Message);
            }

            return false;
        }
        private void DeferRemoveSubjectTokensAsync(string subjectId)
        {

            _taskQueueDelete.QueueBackgroundWorkItem(x =>
            {
                return _persistedGrantStore.RemoveAllAsync(new PersistedGrantFilter
                {
                    SubjectId = subjectId,
                    ClientId = null,
                    Type = null
                });
            });

        }
        public class Delete
        {
        }
    }
}
