using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionDelinking;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus
{
    public class CampaignPromotionDelinkMessageSubsciber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignPromotionDelinkMessageSubsciber> _logger;
        public CampaignPromotionDelinkMessageSubsciber(IMediator mediator, ILogger<CampaignPromotionDelinkMessageSubsciber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [Function("CampaignPromotionDelinkMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.CampaignManagementEventTopicName, Constants.Application.CampaignPromotionDelinkEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            _logger.LogInformation("CampaignPromotionDelinkEvent received");
            await _mediator.Send(Json.Deserialize<CampaignPromotionDelinkEventProcessCommand>(eventRequestPayload));
        }
    }
}
