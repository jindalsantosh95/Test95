using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderCreate;

namespace VAA.EAPI.Microservices.Retail.Order.Triggers.ServiceBus
{
    public class OrderMessageSubscriber
    {
        private readonly IMediator _mediator;
        public OrderMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("OrderMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.OrderEventHandlerTopicName, Constants.Application.OrderEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var orderEventProcessCommand = Json.Deserialize<OrderEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(orderEventProcessCommand);

            await _mediator.Send(orderEventProcessCommand);
        }
    }
}
