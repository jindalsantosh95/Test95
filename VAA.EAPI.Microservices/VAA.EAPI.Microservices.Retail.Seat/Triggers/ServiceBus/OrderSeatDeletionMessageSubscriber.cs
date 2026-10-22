using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Seat.Application.Command;

namespace VAA.EAPI.Microservices.Retail.Seat.Triggers.ServiceBus
{
    public class OrderSeatDeletionMessageSubscriber
    {
        private readonly IMediator _mediator;

        public OrderSeatDeletionMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("OrderSeatDeletionMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.OrderChangeSeatDeleteServiceBusTopicName, Constants.Application.OrderSeatDeletionEventSubscriptionName, Connection = "OrderChangeServiceBusConnectionString")] string eventRequestPayload)
        {
            var cloudEvent = Json.Deserialize<OrderSeatDeletionEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
