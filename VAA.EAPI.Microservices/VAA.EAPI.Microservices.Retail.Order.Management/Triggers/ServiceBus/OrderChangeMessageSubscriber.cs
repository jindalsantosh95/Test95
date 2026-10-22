using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Triggers.ServiceBus
{
    public class OrderChangeMessageSubscriber
    {
        private readonly IMediator _mediator;

        public OrderChangeMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("OrderChangeMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.OrderChangeEventHandlerTopicName, Constants.Application.OrderChangeEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("OrderChangeMessageSubscriber");

            log.LogDebug("OrderChangeEvent received");

            var cloudEvent = Json.Deserialize<OrderChangeEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
