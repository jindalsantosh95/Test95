using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.Seat;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Triggers.ServiceBus
{
    public class OrderSeatFulfilmentMessageSubscriber
    {
        private readonly IMediator _mediator;

        public OrderSeatFulfilmentMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("OrderSeatFulfilmentMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.OrderSeatEventHandlerTopicName, Constants.Application.OrderSeatEventSubscriptionName, Connection = "SeatServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<OrderSeatFulfilmentRequestCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
