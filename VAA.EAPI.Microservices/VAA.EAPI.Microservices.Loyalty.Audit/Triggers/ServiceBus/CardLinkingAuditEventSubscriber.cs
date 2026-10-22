using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.CardLinkingAudit;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Triggers.ServiceBus
{
    public class CardLinkingAuditEventSubscriber
    {
        private readonly IMediator _mediator;

        public CardLinkingAuditEventSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [Function("CardLinkingAuditMessageSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.AuditEventTopicName, Constants.Application.CardLinkingAuditSubscriberName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<CardLinkingAuditCreateEvent>(eventRequestPayload));
        }
    }
}
