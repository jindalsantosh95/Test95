using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Application.Commands;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Triggers.ServiceBus
{
    public class OrderCreateMessageSubscriber
    {
        private readonly IMediator _mediator;

        public OrderCreateMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("OrderCreateMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.OrderEventHandlerTopicName, Constants.Application.OrderEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var cloudEvent = Json.Deserialize<OrderCreateEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
