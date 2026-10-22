using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.FlyingClubToken;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.FlyingClubToken
{
    public sealed class FlyingClubTokenHandler : IFlyingClubTokenHandler
    {
        private const int DefaultLifetimeSeconds = 300;
        private const string FlyingClubAccountNumberPattern = "^[0-9]{1,20}$";
        private const string InvalidAccountNumberCode = "1028";
        private const string InvalidAccountNumberMessage = "Flying Club account number must contain between 1 and 20 digits.";
        private const string UnauthorizedCallerMessage = "The calling application is not authorized.";
        private const string IssuanceFailureMessage = "Flying Club broker-token issuance failed.";
        private readonly IFlyingClubTokenSigner _tokenSigner;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<FlyingClubTokenHandler> _logger;

        public FlyingClubTokenHandler(IFlyingClubTokenSigner tokenSigner, TimeProvider timeProvider, ILogger<FlyingClubTokenHandler> logger)
        {
            _tokenSigner = tokenSigner ?? throw new ArgumentNullException(nameof(tokenSigner));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<FlyingClubTokenResponse>>> HandleAsync(FlyingClubTokenRequest request, CancellationToken cancellationToken)
        {
            var accountNumber = request?.FlyingClubAccountNumber?.Trim();
            if (accountNumber.IsNullOrWhiteSpace() || !RegexCompiled.IsMatch(accountNumber, FlyingClubAccountNumberPattern, RegexOptions.CultureInvariant))
            {
                return Result.Success(new Response<FlyingClubTokenResponse>
                {
                    Status = Status.BadRequest,
                    Errors = new List<ErrorDetails>
                    {
                        new()
                        {
                            Code = InvalidAccountNumberCode,
                            Message = InvalidAccountNumberMessage,
                            ErrorSource = "Consuming application"
                        }
                    }
                });
            }

            try
            {
                var issuer = ConfigReader.Get(Environment.FlyingClubTokenIssuer);
                var audience = ConfigReader.Get(Environment.FlyingClubTokenAudience);
                var claimType = ConfigReader.Get(Environment.FlyingClubTokenClaimType);
                var authorizedParty = ConfigReader.Get(Environment.FlyingClubTokenAuthorizedParty);

                if (issuer.IsNullOrWhiteSpace())
                    throw new InvalidOperationException("The Flying Club token issuer is not configured.");
                if (audience.IsNullOrWhiteSpace())
                    throw new InvalidOperationException("The Flying Club token audience is not configured.");
                if (claimType.IsNullOrWhiteSpace())
                    throw new InvalidOperationException("The Flying Club token claim type is not configured.");
                if (authorizedParty.IsNullOrWhiteSpace())
                    throw new InvalidOperationException("The Flying Club token authorized party is not configured.");

                if (request.CallerApplicationId.IsNullOrWhiteSpace() ||
                    request.AuthenticatedCallerApplicationId.IsNullOrWhiteSpace() ||
                    !request.CallerApplicationId.Is(authorizedParty) ||
                    !request.AuthenticatedCallerApplicationId.Is(authorizedParty))
                {
                    return Result.Success(new Response<FlyingClubTokenResponse>
                    {
                        Status = Status.Forbidden,
                        Errors = new List<ErrorDetails>
                        {
                            new()
                            {
                                Code = "403",
                                Message = UnauthorizedCallerMessage,
                                ErrorSource = "Authentication context"
                            }
                        }
                    });
                }

                var configuredLifetime = ConfigReader.Try(Environment.FlyingClubTokenLifetimeSeconds).Parse(DefaultLifetimeSeconds);
                var lifetimeSeconds = configuredLifetime > 0 ? configuredLifetime : DefaultLifetimeSeconds;

                var now = _timeProvider.GetUtcNow();
                var claims = new Dictionary<string, object>
                {
                    ["iss"] = issuer,
                    ["aud"] = audience,
                    [Constants.Application.AuthorizedPartyClaim] = authorizedParty,
                    [claimType] = accountNumber,
                    ["iat"] = now.ToUnixTimeSeconds(),
                    ["nbf"] = now.ToUnixTimeSeconds(),
                    ["exp"] = now.AddSeconds(lifetimeSeconds).ToUnixTimeSeconds(),
                    ["jti"] = Guid.NewGuid().ToString("N")
                };

                var token = await _tokenSigner.SignAsync(claims, cancellationToken);
                return Result.Success(new Response<FlyingClubTokenResponse>
                {
                    Status = Status.OK,
                    Value = new FlyingClubTokenResponse
                    {
                        AccessToken = token,
                        TokenType = "Bearer",
                        ExpiresIn = lifetimeSeconds
                    }
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, IssuanceFailureMessage);
                return Result.Failure<Response<FlyingClubTokenResponse>>(IssuanceFailureMessage);
            }
        }
    }
}
