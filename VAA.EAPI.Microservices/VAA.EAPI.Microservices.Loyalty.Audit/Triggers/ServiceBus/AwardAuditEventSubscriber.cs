using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.Award;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Triggers.ServiceBus
{
    public class AwardAuditEventSubscriber

    {
        private readonly IMediator _mediator;

        public AwardAuditEventSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [Function("CreateAwardAuditMessageSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.AuditEventTopicName, Constants.Application.AwardAuditEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<AwardAuditCreateEvent>(eventRequestPayload));
        }
    }
}
