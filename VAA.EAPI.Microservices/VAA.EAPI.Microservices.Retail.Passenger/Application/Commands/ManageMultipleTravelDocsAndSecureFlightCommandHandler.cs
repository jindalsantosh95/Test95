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
    public class ManageMultipleTravelDocsAndSecureFlightCommandHandler : IRequestHandler<ManageMultipleTravelDocsAndSecureFlightCommand, Result<Response<ManageMultipleTravelDocsAndSecureFlightResponseType>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ManageMultipleTravelDocsAndSecureFlightCommandHandler> _logger;
        private readonly ICacheConfig _cacheConfig;

        public ManageMultipleTravelDocsAndSecureFlightCommandHandler(IMediator mediator, ILogger<ManageMultipleTravelDocsAndSecureFlightCommandHandler> logger, ICacheConfig cacheConfig)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheConfig = cacheConfig ?? throw new ArgumentNullException(nameof(cacheConfig));

        }

        public async Task<Result<Response<ManageMultipleTravelDocsAndSecureFlightResponseType>>> Handle(ManageMultipleTravelDocsAndSecureFlightCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var channelEntity = await _cacheConfig.GetChannelEntity(request.ChannelId);
                if (channelEntity != null)
                {
                    request.RequestInfo.ApplicationId = channelEntity.DlApplicationId;
                    request.RequestInfo.AppChannelName = channelEntity.DlApplicationName;

                    Response<ManageMultipleTravelDocsAndSecureFlightResponseType> response = new Response<ManageMultipleTravelDocsAndSecureFlightResponseType>()
                    {
                        Value = await _mediator.Send(new ManageMultipleTravelDocsAndSecureFlightAir4Command { ChannelId = request.ChannelId, DocumentDetailData = request.DocumentDetailData, PnrRequestParams = request.PnrRequestParams, FlightInfo = request.FlightInfo, GovtIssuedNumber = request.GovtIssuedNumber, PassengerContactData = request.PassengerContactData, PassengerNameItem = request.PassengerNameItem, SecureFlightPassengerData = request.SecureFlightPassengerData, SetupContextRequest = request.SetupContextRequest, TeardownContextRequest = request.TeardownContextRequest, RequestInfo = request.RequestInfo, StampingInfo = request.StampingInfo }),
                        Status = Status.OK
                    };
                    return Result.Success(response);
                }
                else
                {
                    return Result.Success(new Response<ManageMultipleTravelDocsAndSecureFlightResponseType> { Errors = ErrorMapper.MapBusinessExceptionToEAPIException(ex: new Exception(ErrorCodes.InvalidChannelErrorCode), guid: AsyncContext.Current.CorrelationId.ToString(), errorFilePath: EAPI.Infrastructure.Constants.Constants.ErrorCodesPath), Status = Status.BadRequest, IgnoreNullValues = false });
                }
            }
            catch (ExternalServiceIntegrationException ex)
            {
                _logger.LogError(ex, $"Error occured in ManageMultipleTravelDocsAndSecureFlight Command Handler || CorrelationId : {AsyncContext.Current.CorrelationId}");
                var error = ErrorMapper.MapDeltaExceptionToEAPIException(ex.Message, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message);
                return Result.Failure<Response<ManageMultipleTravelDocsAndSecureFlightResponseType>>(Json.Serialize(error));
            }
        }
    }
}
