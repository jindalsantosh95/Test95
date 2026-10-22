using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Triggers.ServiceBus
{
    public class UpdateOCDBSyncMessageSubscriber
    {
        private readonly IMediator _mediator;

        public UpdateOCDBSyncMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("UpdateOCDBSyncMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.ManagaeAccountUpdateTopicName, Constants.Application.UpdateOCDBEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<UpdateOCDBSyncCommand>(eventRequestPayload));
        }
    }
}
