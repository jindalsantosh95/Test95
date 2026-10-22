using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Events;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Triggers.ServiceBus
{
    public class OrderChangeMessageSubscriber
    {
        private readonly IMediator _mediator;

        public OrderChangeMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("OrderChangeMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.OrderChangeEventHandlerTopicName, Constants.Application.OrderChangeEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var cloudEvent = Json.Deserialize<OrderChangeEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
