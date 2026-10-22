using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Triggers.ServiceBus
{
    public class DeleteOrderChangeEventStoreMessageSubscriber
    {
        private readonly IMediator _mediator;

        public DeleteOrderChangeEventStoreMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("DeleteOrderEventStoreMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.DeleteOrderMgmtEventStoreHandlerTopicName, Constants.Application.DeleteOrdermgmtEventStoreSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var cloudEvent = Json.Deserialize<DeleteOrderChangeEventStoreCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}

