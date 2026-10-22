using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Triggers.ServiceBus
{
    public class LoyaltySyncMessageSubscriber
    {
        private readonly IMediator _mediator;
        public LoyaltySyncMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("LoyaltySyncMessageSubscriber")]
        public async Task Run([Microsoft.Azure.Functions.Worker.ServiceBusTrigger(Constants.Environment.LoyaltyEventHandlerTopicName, Constants.Environment.LoyaltyEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("LoyaltySyncMessageSubscriber");
            log.LogInformation("Loyalty Sync Event received");
            await _mediator.Send(Json.Deserialize<LoyaltySyncEventProcessCommand>(eventRequestPayload));
        }
    }
}
