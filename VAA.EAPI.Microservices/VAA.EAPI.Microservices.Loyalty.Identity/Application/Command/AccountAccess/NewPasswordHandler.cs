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
    public class NewPasswordHandler : INewPasswordHandler
    {
        private readonly ILogger<NewPasswordHandler> _logger;
        private readonly IEntraService _entraService;

        public NewPasswordHandler(ILogger<NewPasswordHandler> logger, IEntraService entraService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _entraService = entraService ?? throw new ArgumentNullException(nameof(entraService));
        }

        public async Task<Response<TokenResponse>> AccountSubmitNewPassword(NewPasswordRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Request is null");

            if (string.IsNullOrWhiteSpace(request.ContinuationToken))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Continuation token is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Password is required");

            try
            {
                var continuationToken = request.ContinuationToken;
                const int maxRetries = 5;
                const int maxWaitTimeSeconds = 60; 
                int totalWaitTime = 0;
                int attemptCount = 0;

                while (attemptCount < maxRetries && totalWaitTime < maxWaitTimeSeconds)
                {
                    var submitResponse = await _entraService.Submit(continuationToken, request.Password, cancellationToken);
                    var submitJson = JsonConvert.DeserializeObject<JObject>(submitResponse);

                    if (submitJson == null || submitJson["error"] != null)
                        return EntraErrorHelper<TokenResponse>.CreateErrorResponse(submitJson);

                    continuationToken = submitJson["continuation_token"]?.ToString();

                    if (string.IsNullOrEmpty(continuationToken))
                        return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "invalid_response", "Invalid submit response: missing continuation token");

                    int pollInterval = submitJson["poll_interval"]?.Value<int>() ?? 2; 

                    bool shouldRetry = false;
                    do
                    {
                        await Task.Delay(TimeSpan.FromSeconds(pollInterval), cancellationToken);
                        totalWaitTime += pollInterval;

                        var pollResponse = await _entraService.PollCommunication(continuationToken, cancellationToken);
                        var pollJson = JsonConvert.DeserializeObject<JObject>(pollResponse);

                        if (pollJson == null || pollJson["error"] != null)
                            return EntraErrorHelper<TokenResponse>.CreateErrorResponse(pollJson);

                        string status = pollJson["status"]?.ToString();
                        continuationToken = pollJson["continuation_token"]?.ToString() ?? continuationToken;

                        switch (status)
                        {
                            case "succeeded":
                                var scopes = new List<string> { "openid", "offline_access", "profile", "email"};
                                var tokenResponse = await _entraService.Token(continuationToken, new List<string> { "continuation_token" }, scopes, null, request.Password, null, cancellationToken);

                                var tokenJson = JsonConvert.DeserializeObject<JObject>(tokenResponse);

                                if (tokenJson == null || tokenJson["error"] != null)
                                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(tokenJson);

                                return new Response<TokenResponse>
                                {
                                    Status = Status.OK,
                                    Value = new TokenResponse
                                    {
                                        AccessToken = tokenJson["access_token"]?.ToString(),
                                        RefreshToken = tokenJson["refresh_token"]?.ToString(),
                                        IdToken = tokenJson["id_token"]?.ToString(),
                                        ExpiresIn = tokenJson["expires_in"]?.ToObject<int>() ?? 0,
                                        TokenType = tokenJson["token_type"]?.ToString()
                                    }
                                };

                            case "failed":
                                shouldRetry = false;
                                break;

                            case "not_started":
                            case "in_progress":
                                shouldRetry = true;
                                break;

                            default:
                                _logger.LogWarning("Unknown status '{Status}' received from Entra service", status);
                                shouldRetry = false;
                                break;
                        }
                    } while (shouldRetry && totalWaitTime < maxWaitTimeSeconds);

                    attemptCount++;
                }

                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "timeout",
                    "Password reset process did not complete within the expected time frame");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during password change process");
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unexpected_error", "An unexpected error occurred during password change");
            }
        }
    }
}
