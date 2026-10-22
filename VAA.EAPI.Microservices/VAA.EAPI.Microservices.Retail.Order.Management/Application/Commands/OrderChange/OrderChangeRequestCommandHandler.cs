using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Validations;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange
{

    public class OrderChangeRequestCommandHandler : IRequestHandler<OrderChangeRequestCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderChangeRequestCommandHandler> _logger;
        private readonly ICachedChannel _cachedChannel;
        public OrderChangeRequestCommandHandler(IMediator mediator, ILogger<OrderChangeRequestCommandHandler> logger, ICachedChannel cachedChannel)
        {
            _mediator = mediator;
            _cachedChannel = cachedChannel;
            _logger = logger;
        }
        public async Task<Result<Response<string>>> Handle(OrderChangeRequestCommand request, CancellationToken cancellationToken)
        {
            bool eventCreated = false;
            var id = string.Empty;

            try
            {
                var channelEntity = await _cachedChannel.GetChannelEntity(request.ChannelId);
                if (channelEntity != null)
                {
                    var cloudEvent = OrderChangeCommandToEventMapper.CommandToEventMapper(request);

                    id = cloudEvent.Id;

                    _logger.LogDebug($"Request validation - Start");
                    var result = OrderChangeRequestValidator.ValidateRequest(request);
                    _logger.LogDebug($"Request validation - End");
                    if (result.Value.Status == Status.OK)
                    {
                        _logger.LogDebug($"Store event in event store - Start");
                        await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });
                        _logger.LogDebug($"Store event in event store - End");
                        _logger.LogDebug($"Publish event - Start");
                        await _mediator.Publish(cloudEvent);
                        _logger.LogDebug($"Publish event - End");
                        var orderChangeResponse = OrderChangeEventCommandResponseMapper.OrderResponseMapper(cloudEvent.Id, Convert.ToString(HttpStatusCode.Accepted));
                        return orderChangeResponse != null ? Result.Success(orderChangeResponse) : Result.Failure<Response<string>>(Convert.ToString(HttpStatusCode.InternalServerError));
                    }
                    else
                    {
                        return Result.Success(new Response<string>
                        {
                            Errors = result.Value.Errors,
                            Status = Status.BusinessException
                        });
                    }
                }
                else
                {
                    return Result.Success(new Response<string>
                    {
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.InvalidChannelErrorCode, id, EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                        Status = Status.BusinessException
                    });

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback from event store for CorrelationId: {AsyncContext.Current.CorrelationId} due to error in OrderChangeRequestCommandHandler handler");
                if (eventCreated)
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = id, Type = OrderEventType.OrderChangeInitiate.ToString(), Data = request, Subject = Constants.Application.OrderChangeEventSubject });
                return Result.Failure<Response<string>>(ex.Message);

            }
        }
    }
}
