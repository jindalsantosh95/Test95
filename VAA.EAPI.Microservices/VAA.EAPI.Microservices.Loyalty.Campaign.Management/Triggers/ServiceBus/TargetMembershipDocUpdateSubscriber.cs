using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.TargetedMembership;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus
{
    public class TargetedMembershipDocSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TargetedMembershipDocSubscriber> _logger;
        public TargetedMembershipDocSubscriber(IMediator mediator, ILogger<TargetedMembershipDocSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [Function("TargetMembershipDocUpdateSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.CampaignManagementEventTopicName, Constants.Application.TargetedMembershipDocUpdateSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            _logger.LogInformation("CampaignRegisterEvent received");
            await _mediator.Send(Json.Deserialize<TargetedMembershipDocUpdateCommand>(eventRequestPayload));
        }
    }
}


