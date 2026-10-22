using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.Seat;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Triggers.ServiceBus
{
    public class OrderSeatDeletionMessageSubscriber(IMediator mediator)
    {
        private readonly IMediator _mediator = mediator;

        [Function("OrderSeatDeletionMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.OrderSeatDeleteEventHandlerTopicName, Constants.Application.OrderSeatDeleteEventSubscriptionName, Connection = "SeatServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<OrderSeatDeletionRequestCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
