using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Models;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Microservices.Retail.Order.Search.Application.Query;
using VAA.EAPI.Microservices.Retail.Order.Search.Constants;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Application.Query
{
    public class PNRRetrieveQueryHandler : IRequestHandler<PNRRetrieveQuery, Result<Response<PNRCommonV6RetrieveResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PNRRetrieveQueryHandler> _logger;

        public PNRRetrieveQueryHandler(IMediator mediator, ILogger<PNRRetrieveQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<PNRCommonV6RetrieveResponse>>> Handle(PNRRetrieveQuery requestData, CancellationToken cancellationToken)
        {
            if (requestData.PNR is null) throw new ArgumentNullException(nameof(requestData.PNR));

            try
            {
                var serviceResponse = await _mediator.Send(new PNRRetrieveRepoQuery(PNRRetrieveMapper.PNRRetrieveRequestMapper(requestData.PNR)), cancellationToken);

                var pnrRetriveResponse = PNRRetrieveHistoryRSMapper.MapToPnrRetrieveResponse(serviceResponse);

                return new Response<PNRCommonV6RetrieveResponse>
                {
                    Status = Status.OK,
                    Value = pnrRetriveResponse
                };
            }
            catch (ExternalServiceIntegrationException ex)
            {
                _logger.LogError("Delta PNR retrieval failed");
                if (ex.InnerException.Message.Contains(Constants.Application.InvalidPNRErrorMessage))
                    return Result.Failure<Response<PNRCommonV6RetrieveResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.PNRNotFoundErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                else
                    return Result.Failure<Response<PNRCommonV6RetrieveResponse>>(Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.ExceptionErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message)));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An unexpected error has occured in PNR Retrieve Query Handler");
                return Result.Failure<Response<PNRCommonV6RetrieveResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
