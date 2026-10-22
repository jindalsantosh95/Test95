using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignPromotionLinking;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus
{
    public class CampaignPromotionLinkMessageSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignPromotionLinkMessageSubscriber> _logger;
        public CampaignPromotionLinkMessageSubscriber(IMediator mediator, ILogger<CampaignPromotionLinkMessageSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [Function("CampaignPromotionLinkMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.CampaignManagementEventTopicName, Constants.Application.CampaignPromotionLinkEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("CampaignPromotionLinkEvent received");
            await _mediator.Send(Json.Deserialize<CampaignPromotionLinkEventProcessCommand>(eventRequestPayload));
        }
    }
}
