using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.OrderCancelUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus
{
    public class CancelOrderDocumentSubscriber
    {
        private readonly IMediator _mediator;

        public CancelOrderDocumentSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function(nameof(CancelOrderDocumentSubscriber))]
        public async Task Run([ServiceBusTrigger(Constants.Environment.CancelOrderSubscriberTopicName, Constants.Environment.CancelOrderSubscriptionName, Connection = "OrderServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<CancelOrderUpdateCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
