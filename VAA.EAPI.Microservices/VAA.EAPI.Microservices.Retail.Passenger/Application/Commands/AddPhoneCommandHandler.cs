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
    public class AddPhoneCommandHandler : IRequestHandler<AddPhoneCommand, Result<Response<AddPhoneResponseType>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AddPhoneCommandHandler> _logger;
        private readonly ICacheConfig _cacheConfig;

        public AddPhoneCommandHandler(IMediator mediator, ILogger<AddPhoneCommandHandler> logger, ICacheConfig cacheConfig)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheConfig = cacheConfig ?? throw new ArgumentNullException(nameof(cacheConfig));

        }

        public async Task<Result<Response<AddPhoneResponseType>>> Handle(AddPhoneCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var channelEntity = await _cacheConfig.GetChannelEntity(request.ChannelId);
                if (channelEntity != null)
                {
                    request.RequestInfo.ApplicationId = channelEntity.DlApplicationId;
                    request.RequestInfo.AppChannelName = channelEntity.DlApplicationName;
                    Response<AddPhoneResponseType> response = new Response<AddPhoneResponseType>()
                    {
                        Value = await _mediator.Send(new AddPhoneAir4Command { ChannelId = request.ChannelId, RequestInfo = request.RequestInfo, StampingInfo = request.StampingInfo, PhoneInfo = request.PhoneInfo, PnrRequestParams = request.PnrRequestParams, RequestOption = request.RequestOption }),
                        Status = Status.OK
                    };
                    return Result.Success(response);
                }
                else
                {
                    return Result.Success(new Response<AddPhoneResponseType> { Errors = ErrorMapper.MapBusinessExceptionToEAPIException(ex: new Exception(ErrorCodes.InvalidChannelErrorCode), guid: AsyncContext.Current.CorrelationId.ToString(), errorFilePath: EAPI.Infrastructure.Constants.Constants.ErrorCodesPath), Status = Status.BadRequest, IgnoreNullValues = false });

                }

            }
            catch (ExternalServiceIntegrationException ex)
            {
                _logger.LogError(ex, $"Error occured in AddPhoneCommandHandler || ErrorId : {AsyncContext.Current.CorrelationId}");
                var error = ErrorMapper.MapDeltaExceptionToEAPIException(ex.Message, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message);
                return Result.Failure<Response<AddPhoneResponseType>>(Json.Serialize(error));
            }
        }
    }
}
