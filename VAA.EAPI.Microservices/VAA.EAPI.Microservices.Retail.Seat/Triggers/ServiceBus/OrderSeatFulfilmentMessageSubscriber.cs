using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Seat.Application.Command;

namespace VAA.EAPI.Microservices.Retail.Seat.Triggers.ServiceBus
{
    public class OrderSeatFulfilmentMessageSubscriber
    {
        private readonly IMediator _mediator;

        public OrderSeatFulfilmentMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("OrderSeatFulfilmentMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.OrderChangeSeatAddServiceBusTopicName, Constants.Application.OrderSeatFulfilmentEventSubscriptionName, Connection = "OrderChangeServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<OrderSeatFulfilmentEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
