using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.OrderCreate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus
{
    public class OrderCreateMessageSubscriber
    {
        private readonly IMediator _mediator;

        public OrderCreateMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("OrderCreateMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Environment.OrderCreateEventHandlerTopicName, Constants.Environment.OrderCreateSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {

            var cloudEvent = Json.Deserialize<OrderCreateRequestCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}

