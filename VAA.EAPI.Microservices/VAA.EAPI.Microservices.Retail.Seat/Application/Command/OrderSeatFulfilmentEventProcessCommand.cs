using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Repositories.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common;

namespace VAA.EAPI.Microservices.Retail.Seat.Application.Command
{
    public class OrderSeatFulfilmentEventProcessCommand : EAPI.Domain.Events.CloudEvent, IRequest<Result<Unit>>
    {
    }

    public class OrderSeatFulfilmentEventProcessCommandHandler : IRequestHandler<OrderSeatFulfilmentEventProcessCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderSeatFulfilmentEventProcessCommand> _logger;

        public OrderSeatFulfilmentEventProcessCommandHandler(IMediator mediator, ILogger<OrderSeatFulfilmentEventProcessCommand> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(OrderSeatFulfilmentEventProcessCommand request, CancellationToken cancellationToken)
        {
            var result = new AddResponseType();

            try
            {
                var query = Json.Deserialize<OrderSeatV8AddSeatQuery>(request.Data.ToString());
                result = await _mediator.Send(query, cancellationToken);

                var seatBookingTime = DateTime.UtcNow.ToString(Constants.Application.IsoDateTimeFormat);
                var cloudEvent = OrderSeatFulfilmentCommandToEventMapper.CommandToEventMapper(SeatFulFilmentResponseMapper.MapAddSeatResponse([result], null, null,seatBookingTime), request.Id);
                await _mediator.Publish(cloudEvent);

                if (result.SeatSuccess != null && query.ChannelId == Constants.Application.NdcChannel)
                    await _mediator.Send(new ServiceBusTopicMessageEventCommand() { Message = SeatFulfilmentCommandToEventMapper.CommandToEventMapper(SeatFulfilmentOrderDocUpdateMapper.UpdateOrderDocMapper(result,query, seatBookingTime), request.Id), ServiceBusTopicName = Constants.Application.NDCUpdateSeatTopicName, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderServiceBusConnectionString) }, cancellationToken);
            }
            catch (ExternalServiceIntegrationException ex)
            {               
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex}");

                var cloudEvent = OrderSeatFulfilmentCommandToEventMapper.CommandToEventMapper(SeatFulFilmentResponseMapper.MapAddSeatResponse([result]), request.Id);
                await _mediator.Publish(cloudEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Channel - {string.Join(',', AsyncContext.Current.Channel)} Exception - {ex}");

                var cloudEvent = OrderSeatFulfilmentCommandToEventMapper.CommandToEventMapper(SeatFulFilmentResponseMapper.MapAddSeatResponse([result]), request.Id);
                await _mediator.Publish(cloudEvent);
            }

            return Unit.Value;
        }
    }
}
