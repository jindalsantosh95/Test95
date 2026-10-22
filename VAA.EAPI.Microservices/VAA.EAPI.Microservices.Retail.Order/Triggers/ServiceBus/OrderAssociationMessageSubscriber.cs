using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderAssociation;

namespace VAA.EAPI.Microservices.Retail.Order.Triggers.ServiceBus
{
    public class OrderAssociationMessageSubscriber
    {
        private readonly IMediator _mediator;

        public OrderAssociationMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("OrderAssociationEventSubscriber")]
        public async Task AssociateOrderMessageSubscriber([ServiceBusTrigger(Constants.Application.OrderAssocaitionEventHandlerTopicName, Constants.Application.OrderAssocaitionEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("OrderAssociationMessageSubscriber");

            log.LogInformation($"OrderAssociationEvent received : {eventRequestPayload}");

            var cloudEvent = Json.Deserialize<OrderAssociationEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent, CancellationToken.None);
        }
    }
}
