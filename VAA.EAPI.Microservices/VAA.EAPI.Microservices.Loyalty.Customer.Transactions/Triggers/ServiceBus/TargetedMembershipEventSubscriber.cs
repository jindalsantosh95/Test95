using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.TargetedMemberships;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class TargetedMembershipEventSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TargetedMembershipEventSubscriber> _logger;

        public TargetedMembershipEventSubscriber(IMediator mediator, ILogger<TargetedMembershipEventSubscriber> logger)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [Function("TargetedMembershipMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.TargetedMembershipEventTopicName, Constants.Application.TargetedMembershipCreateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("TargetedMembershipMessageSubscriber");
            log.LogInformation("TargetedMembershipMessageSubscriber");
            await _mediator.Send(Json.Deserialize<TargetedMembershipCreateEvent>(eventRequestPayload));
        }

        [Function("DeleteTargetedMembershipMessageSubscriber")]
        public async Task Delete([ServiceBusTrigger(Constants.Application.TargetedMembershipEventTopicName, Constants.Application.TargetedMembershipDeleteEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("TargetedMembershipMessageSubscriber");
            log.LogInformation("DeleteTargetedMembershipMessageSubscriber");
            await _mediator.Send(Json.Deserialize<TargetedMembershipDeleteEvent>(eventRequestPayload));
        }

        [Function("UpdateTargetedMembershipMessageSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.TargetedMembershipEventTopicName, Constants.Application.TargetedMembershipUpdateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("TargetedMembershipMessageSubscriber");
            log.LogInformation("UpdateTargetedMembershipMessageSubscriber");
            await _mediator.Send(Json.Deserialize<TargetedMembershipPatchEvent>(eventRequestPayload));
        }

    }
}
