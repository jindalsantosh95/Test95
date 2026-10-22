using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.PreferenceAudit;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Triggers.ServiceBus
{
    public class PreferenceAuditEventSubscriber
    {
        private readonly IMediator _mediator;
        public PreferenceAuditEventSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("CreatePreferenceAuditSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.AuditEventTopicName, Constants.Application.CreatePreferenceAuditEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            await _mediator.Send(Json.Deserialize<PreferenceAuditCreateEvent>(eventRequestPayload));
        }
    }
}