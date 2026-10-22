using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Security;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Microservices.Loyalty.Identity.Constants;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AcquireToken;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;
using VAA.EAPI.Microservices.Loyalty.Identity.Validation;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class AcquireTokenHandler : IRequestHandler<AcquireTokenRequest, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AcquireTokenHandler> _logger;
        private readonly ITokenService<Claim> _tokenService;

        public AcquireTokenHandler(IMediator mediator, ILogger<AcquireTokenHandler> logger, ITokenService<Claim> tokenService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public async Task<Result<Response<string>>> Handle(AcquireTokenRequest request, CancellationToken cancellationToken)
        {
            try
            {
                List<Claim> claims = new();

                var result = ValidateRequest.RequestValidator(request);

                if (result.Value.Status == Status.OK)
                {
                    return string.Equals(request.Type, Constants.Application.TicketType, StringComparison.OrdinalIgnoreCase) ? await TicketingDeltaService(request, claims, cancellationToken) : await PNRRetriveDeltaService(request, claims, cancellationToken);
                }

                _logger.LogDebug("One of the mandatory field is missing in the request");

                return Result.Success(new Response<string>
                {
                    Errors = result.Value.Errors,
                    Status = Status.Unauthorized
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred", ex);

                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

        public async Task<Result<Response<string>>> TicketingDeltaService(AcquireTokenRequest request, List<Claim> claims, CancellationToken cancellationToken)
        {
            RetrieveResponseType ticketingResponse;

            try
            {
                ticketingResponse = await _mediator.Send(new TicketingRetrieveRepoQuery(TicketingRetrieveReqRespMapper.TicketingRetrieveRequestMapper(request)), cancellationToken);
            }
            catch (ExternalServiceIntegrationException ex)
            {
                _logger.LogError(ex, "Fault Exception from Ticketing retrieve delta service", ex);

                return Result.Success(new Response<string>
                {
                    Errors = ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.TicketingServiceErrorCode, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message),
                    Status = Status.Unauthorized
                });
            }

            claims = TicketingRetrieveReqRespMapper.ResponseMapper(ticketingResponse, request);

            return (claims?.Count ?? 0) > 0 ? GenerateToken(claims) : GenerateErrorResponse(ErrorCodes.PassengerNameMisMatchForETicket);
        }

        public async Task<Result<Response<string>>> PNRRetriveDeltaService(AcquireTokenRequest request, List<Claim> claims, CancellationToken cancellationToken)
        {
            EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.RetrieveResponseType pnrServiceResponse;

            try
            {
                pnrServiceResponse = await _mediator.Send(new PNRRetrieveRepoQuery(PNRRetrieveReqResMapper.RetrieveRequestMapper(request)), cancellationToken);
            }
            catch (ExternalServiceIntegrationException ex)
            {
                _logger.LogError(ex, "Fault Exception from PNR retrieve delta service", ex);

                if (ex.InnerException?.Message.Contains(Constants.Application.DeltaMessageForInvalidPNR) ?? false)
                {
                    return Result.Success(new Response<string>
                    {
                        Errors = ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.PNRExpiredErrorCode, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, "NO MATCH FOUND"),
                        Status = Status.Unauthorized
                    });
                }

                return Result.Success(new Response<string>
                {
                    Errors = ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.ExceptionErrorCode, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message),
                    Status = Status.Unauthorized
                });
            }

            claims = PNRRetrieveReqResMapper.ResponseMapper(pnrServiceResponse, request);

            return (claims?.Count ?? 0) > 0 ? GenerateToken(claims) : GenerateErrorResponse(ErrorCodes.PassengerNameMisMatchForPNR);
        }

        public Result<Response<string>> GenerateToken(List<Claim> claims)
        {
            return Result.Success(new Response<string>
            {
                Status = Status.OK,
                Value = Constants.Application.OkValue,
                HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.Authorization, Value = $"Bearer {_tokenService.GenerateAccessToken(claims)}" } },
            });
        }

        public Result<Response<string>> GenerateErrorResponse(string errorCode)
        {
            _logger.LogDebug("Name in request doesn't match with name in PNR/Ticket");

            return Result.Success(new Response<string>
            {
                Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(errorCode, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                Status = Status.Unauthorized
            });
        }
    }
}
