using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.CampaignUnassign;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus
{
    public class TargetedMembershipDeleteEventSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TargetedMembershipDeleteEventSubscriber> _logger;

        public TargetedMembershipDeleteEventSubscriber(IMediator mediator, ILogger<TargetedMembershipDeleteEventSubscriber> logger)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [Function("TargetedMembershipDeleteMessageSubscriber")]
        public async Task Delete([ServiceBusTrigger(Constants.Application.TargetedMembershipEventTopicName, Constants.Application.TargetedMembershipDeleteEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("DeleteTargetedMembershipMessageSubscriber");
            await _mediator.Send(Json.Deserialize<TargetedMembershipDeleteEventCommand>(eventRequestPayload));
        }
    }
}
