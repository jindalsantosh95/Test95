using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.CustomerProfile;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Triggers.ServiceBus
{
    public class CustomerProfileAuditEventSubscriber
    {
        private readonly IMediator _mediator;

        public CustomerProfileAuditEventSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("CreateCustomerProfileAuditEventSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.AuditEventTopicName, Constants.Application.CreateCustomerProfileAuditEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<CustomerProfileAuditCreateEvent>(eventRequestPayload));
        }
    }
}