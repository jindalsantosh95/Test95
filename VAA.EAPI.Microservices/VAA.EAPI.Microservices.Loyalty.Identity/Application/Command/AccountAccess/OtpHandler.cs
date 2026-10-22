using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess
{
    public class OtpHandler : IOtpHandler
    {
        private readonly IEntraService _entraService;

        public OtpHandler(IEntraService entraService)
        {
            _entraService = entraService ?? throw new ArgumentNullException(nameof(entraService));
        }

        public async Task<Response<TokenResponse>> SubmitOTP(OtpRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Request is null");

            if (string.IsNullOrWhiteSpace(request.OtpCode))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Otp is required");

            if (string.IsNullOrWhiteSpace(request.ContinuationToken))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Continuation Token is required");

            string tokenResponse = string.Empty;

            if (request.UrlEntraJourney.Is("signin"))
            {
                tokenResponse = await _entraService.Token(request.ContinuationToken, new List<string> { "mfa_oob" }, new List<string> { "openid", "offline_access", "profile", "email", ConfigReader.Get(Environment.EntraAPIScope) }, null, null, request.OtpCode);
            }
            else
            {
                tokenResponse = await _entraService.Continue(request.ContinuationToken, "oob", request.OtpCode, request.UrlEntraJourney, cancellationToken);
            }


            var tokenJson = JsonConvert.DeserializeObject<JObject>(tokenResponse);

            if (tokenJson == null || (tokenJson["error"] != null && tokenJson["continuation_token"] == null))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(tokenJson);

            var continuationToken = tokenJson["continuation_token"]?.ToString();

            return new Response<TokenResponse>
            {
                Status = Status.OK,
                Value = new TokenResponse
                {
                    ContinuationToken = continuationToken,
                    AccessToken = tokenJson["access_token"]?.ToString(),
                    RefreshToken = tokenJson["refresh_token"]?.ToString(),
                    IdToken = tokenJson["id_token"]?.ToString(),
                    ExpiresIn = tokenJson["expires_in"]?.ToObject<int>() ?? 0,
                    Scope = "openid offline_access email profile",
                    TokenType = tokenJson["token_type"]?.ToString()
                }
            };
        }
    }
}
