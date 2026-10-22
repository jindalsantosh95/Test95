using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.LoyaltyGenericAudit;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Triggers.ServiceBus
{
    public class LoyaltyGenericAuditEventSubscriber
    {
        private readonly IMediator _mediator;

        public LoyaltyGenericAuditEventSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("CreateOtherAuditEventSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.AuditEventTopicName, Constants.Application.CreateLoyaltyGenericAuditEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<LoyaltyGenericAuditCreateEvent>(eventRequestPayload));
        }
    }
}