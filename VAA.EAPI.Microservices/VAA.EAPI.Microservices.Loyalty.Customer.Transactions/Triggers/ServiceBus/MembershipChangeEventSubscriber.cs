using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.MembershipChange;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class MembershipChangeEventSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MembershipChangeEventSubscriber> _logger;
        public MembershipChangeEventSubscriber(IMediator mediator, ILogger<MembershipChangeEventSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("CreateMembershipChangeSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.CampaignEnrolmentDocEventTopicName, Constants.Application.CreateMembershipChangeEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("CreateMembershipChangeSubscriber");
            log.LogInformation("CreateMembershipChangeSubscriber");
            await _mediator.Send(Json.Deserialize<MembershipChangeCreateEvent>(eventRequestPayload));
        }

        [Function("UpdateMembershipChangeSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.CampaignEnrolmentDocEventTopicName, Constants.Application.UpdateMembershipChangeEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("UpdateMembershipChangeSubscriber");
            log.LogInformation("UpdateMembershipChangeSubscriber");
            await _mediator.Send(Json.Deserialize<MembershipChangePatchEvent>(eventRequestPayload));
        }

    }
}
