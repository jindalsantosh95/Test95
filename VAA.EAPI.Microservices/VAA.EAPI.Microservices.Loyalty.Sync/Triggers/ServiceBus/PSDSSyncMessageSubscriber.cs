using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Triggers.ServiceBus
{
    public class PSDSSyncMessageSubscriber
    {
        private readonly IMediator _mediator;
        public PSDSSyncMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("PSDSSyncMessageSubscriber")]

        public async Task RunAsync([Microsoft.Azure.Functions.Worker.ServiceBusTrigger(Constants.Environment.LoyaltyEventHandlerTopicName, Constants.Environment.PSDSEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<PSDSSyncEventCommand>(eventRequestPayload));
        }
    }
}
