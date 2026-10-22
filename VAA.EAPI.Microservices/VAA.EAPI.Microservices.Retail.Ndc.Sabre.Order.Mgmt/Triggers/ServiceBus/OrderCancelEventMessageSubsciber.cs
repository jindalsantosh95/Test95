using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Events;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Triggers.ServiceBus
{
    public class OrderCancelEventMessageSubsciber
    {
        private readonly IMediator _mediator;

        public OrderCancelEventMessageSubsciber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("OrderCancelEventMessageSubsciber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.OrderCancelEventHandlerTopicName, Constants.Application.OrderCancelEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var cloudEvent = Json.Deserialize<OrderCancelEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
