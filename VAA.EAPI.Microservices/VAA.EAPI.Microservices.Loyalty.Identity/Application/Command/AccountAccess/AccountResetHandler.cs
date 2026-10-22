using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess
{
    public class AccountResetHandler : IAccountResetHandler
    {
        private readonly ILogger<AccountResetHandler> _logger;
        private readonly IEntraService _entraService;
        private readonly IVerifyMemberStatusRepository _verifyMemberStatusRepository;

        public AccountResetHandler(ILogger<AccountResetHandler> logger, IEntraService entraService, IVerifyMemberStatusRepository verifyMemberStatusRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _entraService = entraService ?? throw new ArgumentNullException(nameof(entraService));
            _verifyMemberStatusRepository = verifyMemberStatusRepository ?? throw new ArgumentNullException(nameof(verifyMemberStatusRepository));
        }

        public async Task<Response<string>> AccountResetPassword(AccountResetRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return EntraErrorHelper<string>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Request is null");

            if (string.IsNullOrWhiteSpace(request.Username))
                return EntraErrorHelper<string>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Username is required");

            try
            {
                // Start the reset process
                var challengeTypes = new List<ChallengeType> { ChallengeType.Oob, ChallengeType.Redirect };

                var startResponse = await _entraService.Start(challengeTypes, "resetpassword", request.Username, cancellationToken);
                var startJson = JsonConvert.DeserializeObject<JObject>(startResponse);

                if (startJson == null || startJson["error"] != null)
                    return EntraErrorHelper<string>.CreateErrorResponse(startJson);

                if (startJson["challenge_type"]?.ToString() == "redirect")
                    return EntraErrorHelper<string>.CreateRedirectResponse();

                var continuationToken = startJson["continuation_token"]?.ToString();
                if (string.IsNullOrEmpty(continuationToken))
                    return EntraErrorHelper<string>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "invalid_response", "Invalid reset response: missing continuation token");

                var customerResponse = await _verifyMemberStatusRepository.VerifyMemberStatus(request.Username, string.Empty, cancellationToken);
                bool isActive = customerResponse?.MemberStatus != null && customerResponse.MemberStatus
                    .Replace(" ", string.Empty)
                    .IsAny("active", "inactive", "activepending");

                if (!isActive) return EntraErrorHelper<string>.CreateErrorResponse(Constants.ErrorCodes.B2CBusinessError, "Account is not Active", "Account did not meet the required status to reset password");

                // Challenge
                var challengeResponse = await _entraService.Challenge(challengeTypes, "resetpassword/v1.0", continuationToken, cancellationToken);
                var challengeJson = JsonConvert.DeserializeObject<JObject>(challengeResponse);

                if (challengeJson == null || challengeJson["error"] != null)
                    return EntraErrorHelper<string>.CreateErrorResponse(challengeJson);

                if (challengeJson["challenge_type"]?.ToString() == "redirect")
                    return EntraErrorHelper<string>.CreateRedirectResponse();

                continuationToken = challengeJson["continuation_token"]?.ToString();
                if (string.IsNullOrEmpty(continuationToken))
                {
                    _logger.LogError("Error Occurred in AccountResetCommandHandler for {Username}: Invalid reset response - missing continuation token after challenge", request.Username);
                    return EntraErrorHelper<string>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unexpected_error", "An unexpected error occurred during reset");
                }

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = continuationToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during reset process for user {Username}", request.Username);
                return EntraErrorHelper<string>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unexpected_error", "An unexpected error occurred during reset");
            }
        }
    }
}