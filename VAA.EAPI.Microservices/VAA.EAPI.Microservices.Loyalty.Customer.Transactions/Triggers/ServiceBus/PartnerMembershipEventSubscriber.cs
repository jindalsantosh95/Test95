using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.PartnerMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class PartnerMembershipEventSubscriber
    {
        private readonly IMediator _mediator;

        public PartnerMembershipEventSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("CreatePartnerMembershipSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.PartnerMembershipEventTopicName, Constants.Application.CreatePartnerMembershipEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<PartnerMembershipCreateEvent>(eventRequestPayload));
        }

        [Function("UpdatePartnerMembershipSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.PartnerMembershipEventTopicName, Constants.Application.UpdatePartnerMembershipEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<PartnerMembershipPatchEvent>(eventRequestPayload));
        }
    }
}
