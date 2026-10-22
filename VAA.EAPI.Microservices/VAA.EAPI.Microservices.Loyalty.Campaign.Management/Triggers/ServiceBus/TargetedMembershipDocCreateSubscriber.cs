using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.TargetedMembership;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus
{
    public class TargetedMembershipDocCreateSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TargetedMembershipDocCreateSubscriber> _logger;
        public TargetedMembershipDocCreateSubscriber(IMediator mediator, ILogger<TargetedMembershipDocCreateSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [Function("TargetedMembershipDocCreateSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.CampaignUpdateEventHandlerTopicName, Constants.Application.TargetedMembershipDocCreateSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            _logger.LogInformation("CampaignAssignEvent received");
            await _mediator.Send(Json.Deserialize<TargetedMembershipDocCreateCommand>(eventRequestPayload));
        }
    }
}
