using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Constants;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountAccess;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Interface;
using VAA.EAPI.Microservices.Loyalty.Identity.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;


namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess
{
    public class RefreshTokenHandler : IRefreshTokenHandler
    {
        private readonly ILogger<RefreshTokenHandler> _logger;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAuthTokenValidator _authTokenValidator;
        private readonly IVerifyMemberStatusRepository _verifyMemberStatusRepository;


        public RefreshTokenHandler(ILogger<RefreshTokenHandler> logger, IRefreshTokenService refreshTokenService, IAuthTokenValidator authTokenValidator, IVerifyMemberStatusRepository verifyMemberStatusRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
            _authTokenValidator = authTokenValidator ?? throw new ArgumentNullException(nameof(authTokenValidator));
            _verifyMemberStatusRepository = verifyMemberStatusRepository ?? throw new ArgumentNullException(nameof(verifyMemberStatusRepository));
        }

        public async Task<Response<TokenResponse>> ValidateRefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Request is null");

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "invalid_request", "Refresh token is required");

            try
            {
                if (request.Scopes.IsNullOrEmpty())
                {
                    request.Scopes = new List<string> { ConfigReader.Get(Environment.EntraAPIScope) };
                }
                else
                {
                    request.Scopes.Append(ConfigReader.Get(Environment.EntraAPIScope));
                }

                var response = await _refreshTokenService.RefreshToken(request.RefreshToken, request.Scopes, cancellationToken);
                var responseJson = JsonConvert.DeserializeObject<JObject>(response);

                if (responseJson == null || responseJson["error"] != null)
                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(responseJson);

                IEnumerable<Claim> verifiedToken = await _authTokenValidator.Verify(responseJson["id_token"]?.ToString());

                if (!verifiedToken.Any()) return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "id_token missing", "No Id token was returned");

                TokenClaimResponse claimResponse = AuthTokenResponseMapper.ResponseMapper(verifiedToken);

                claimResponse.Oid = verifiedToken.FirstOrDefault(claim => claim.Type.Is("oid"))?.Value;
                var customerResponse = await _verifyMemberStatusRepository.VerifyMemberStatus(string.Empty, claimResponse.Oid, cancellationToken);

                if (customerResponse == null) return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.BadRequest, "Member Status missing", "Verify member status failed to return response");

                bool isActive = customerResponse.MemberStatus != null && customerResponse.MemberStatus.Is("active");
                if (!isActive) return new Response<TokenResponse> { Status = Status.OK, ErrorMessage = "Account did not meet the required status to log in" };

                var tokenResponse = new TokenResponse
                {
                    AccessToken = responseJson["access_token"]?.ToString(),
                    RefreshToken = responseJson["refresh_token"]?.ToString(),
                    TokenType = responseJson["token_type"]?.ToString(),
                    Scope = "openid offline_access email profile" ,
                    IdToken = responseJson["id_token"]?.ToString()
                };

                if (int.TryParse(responseJson["expires_in"]?.ToString(), out int expiresIn))
                {
                    tokenResponse.ExpiresIn = expiresIn;
                }

                if (string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    _logger.LogError("Failed to refresh token: Access token missing from response");
                    return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "invalid_response", "Access token missing from response");
                }

                return new Response<TokenResponse>
                {
                    Status = Status.OK,
                    Value = tokenResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token refresh");
                return EntraErrorHelper<TokenResponse>.CreateErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, "unexpected_error", "An unexpected error occurred during token refresh");
            }
        }
    }
}
