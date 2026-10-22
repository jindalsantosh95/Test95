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
    public class DeleteOrderEventStoreMessageSubscriber
    {
        private readonly IMediator _mediator;

        public DeleteOrderEventStoreMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("DeleteOrderEventStoreMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.DeleteOrderEventStoreHandlerTopicName, Constants.Application.DeleteOrderEventStoreSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var cloudEvent = Json.Deserialize<DeleteOrderEventStoreCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
