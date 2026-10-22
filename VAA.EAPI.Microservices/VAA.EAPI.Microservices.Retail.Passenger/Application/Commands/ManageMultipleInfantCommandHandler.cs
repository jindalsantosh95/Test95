using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Passenger.Constants;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository;

namespace VAA.EAPI.Microservices.Retail.Passenger.Application.Commands
{
    public class ManageMultipleInfantCommandHandler : IRequestHandler<ManageMultipleInfantCommand, Result<Response<ManageMultipleInfantV2Response>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ManageMultipleInfantCommandHandler> _logger;
        private readonly ICacheConfig _cacheConfig;

        public ManageMultipleInfantCommandHandler(IMediator mediator, ILogger<ManageMultipleInfantCommandHandler> logger, ICacheConfig cacheConfig)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheConfig = cacheConfig ?? throw new ArgumentNullException(nameof(cacheConfig));
        }

        public async Task<Result<Response<ManageMultipleInfantV2Response>>> Handle(ManageMultipleInfantCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var channelEntity = await _cacheConfig.GetChannelEntity(request.ChannelId);
                if (channelEntity != null)
                {
                    request.RequestInfo.ApplicationId = channelEntity.DlApplicationId;
                    request.RequestInfo.AppChannelName = channelEntity.DlApplicationName;
                    Response<ManageMultipleInfantV2Response> response = new Response<ManageMultipleInfantV2Response>()
                    {
                        Value = await _mediator.Send(new ManageMultipleInfantAir4Command { ChannelId = request.ChannelId, RequestInfo = request.RequestInfo, InfantV2Info = request.InfantV2Info }),
                        Status = Status.OK
                    };
                    return Result.Success(response);
                }
                else
                {
                    return Result.Success(new Response<ManageMultipleInfantV2Response> { Errors = ErrorMapper.MapBusinessExceptionToEAPIException(ex: new Exception(ErrorCodes.InvalidChannelErrorCode), guid: AsyncContext.Current.CorrelationId.ToString(), errorFilePath: EAPI.Infrastructure.Constants.Constants.ErrorCodesPath), Status = Status.BadRequest, IgnoreNullValues = false });
                }
            }
            catch (ExternalServiceIntegrationException ex)
            {
                _logger.LogError(ex, $"Error occured in ManageMultipleTravelDocsAndSecureFlight Command Handler || CorrelationId : {AsyncContext.Current.CorrelationId}");
                var error = ErrorMapper.MapDeltaExceptionToEAPIException(ex.Message, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message);
                return Result.Failure<Response<ManageMultipleInfantV2Response>>(Json.Serialize(error));
            }

        }
    }
}
