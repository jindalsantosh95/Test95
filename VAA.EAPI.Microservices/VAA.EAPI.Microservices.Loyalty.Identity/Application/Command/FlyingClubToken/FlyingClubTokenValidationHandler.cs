using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.FlyingClubToken;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.FlyingClubToken
{
    public sealed class FlyingClubTokenValidationHandler : IFlyingClubTokenValidationHandler
    {
        private const string InvalidTokenMessage = "The Flying Club application token is invalid.";
        private const string ValidationFailureMessage = "Flying Club broker-token validation failed.";
        private readonly IFlyingClubTokenValidator _validator;
        private readonly ILogger<FlyingClubTokenValidationHandler> _logger;

        public FlyingClubTokenValidationHandler(IFlyingClubTokenValidator validator, ILogger<FlyingClubTokenValidationHandler> logger)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<FlyingClubTokenValidationResponse>>> HandleAsync(FlyingClubTokenValidationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var accountNumber = await _validator.ValidateAsync(request?.AccessToken, request?.AuthenticatedCallerApplicationId, cancellationToken);
                return Result.Success(new Response<FlyingClubTokenValidationResponse>
                {
                    Status = Status.OK,
                    Value = new FlyingClubTokenValidationResponse { FlyingClubAccountNumber = accountNumber }
                });
            }
            catch (Exception exception) when (exception is SecurityTokenException or SecurityTokenMalformedException)
            {
                _logger.LogWarning(exception, ValidationFailureMessage);
                return Result.Success(new Response<FlyingClubTokenValidationResponse>
                {
                    Status = Status.Unauthorized,
                    Errors = new List<ErrorDetails>
                    {
                        new()
                        {
                            Code = "401",
                            Message = InvalidTokenMessage,
                            ErrorSource = "Authentication context"
                        }
                    }
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Flying Club broker-token validation failed unexpectedly.");
                return Result.Failure<Response<FlyingClubTokenValidationResponse>>(ValidationFailureMessage);
            }
        }
    }
}
