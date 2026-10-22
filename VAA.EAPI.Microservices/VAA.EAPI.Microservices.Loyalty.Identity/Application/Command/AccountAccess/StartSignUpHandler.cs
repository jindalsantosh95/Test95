using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess
{
    public class StartSignUpHandler : IStartSignUpHandler
    {
        private readonly ILogger<StartSignUpHandler> _logger;
        private readonly IEntraService _entraService;

        public StartSignUpHandler(ILogger<StartSignUpHandler> logger, IEntraService entraService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _entraService = entraService ?? throw new ArgumentNullException(nameof(entraService));
        }

        public async Task<Response<string>> StartSignUp(AccountSignUpRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return EntraErrorHelper<string>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Request is null");

            if (string.IsNullOrWhiteSpace(request.Username))
                return EntraErrorHelper<string>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Username is required");

            try
            {
                var challengeTypes = new List<ChallengeType> { ChallengeType.Oob, ChallengeType.Password, ChallengeType.Redirect };

                // Start the sign-up process
                var signUpResponse = await _entraService.Start(challengeTypes, "signup", request.Username, cancellationToken);
                var signUpJson = JsonConvert.DeserializeObject<JObject>(signUpResponse);

                if (signUpJson == null || signUpJson["error"] != null)
                    return EntraErrorHelper<string>.CreateErrorResponse(signUpJson);

                var continuationToken = signUpJson["continuation_token"]?.ToString();
                if (string.IsNullOrEmpty(continuationToken))
                    return EntraErrorHelper<string>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "invalid_response", "Invalid sign-up response: missing continuation token");

                // Challenge
                var challengeResponse = await _entraService.Challenge(challengeTypes, "signup/v1.0", continuationToken, cancellationToken);
                var challengeJson = JsonConvert.DeserializeObject<JObject>(challengeResponse);

                if (challengeJson == null || challengeJson["error"] != null)
                    return EntraErrorHelper<string>.CreateErrorResponse(challengeJson);

                if (challengeJson["challenge_type"]?.ToString() == "redirect")
                    return EntraErrorHelper<string>.CreateRedirectResponse();

                continuationToken = challengeJson["continuation_token"]?.ToString();
                if (string.IsNullOrEmpty(continuationToken))
                    return EntraErrorHelper<string>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "invalid_response", "Invalid sign-up response: missing continuation token");

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = continuationToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Entra sign-up process failed.");
                return EntraErrorHelper<string>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unexpected_error", "An unexpected error occurred during registration");
            }
        }
    }
}
