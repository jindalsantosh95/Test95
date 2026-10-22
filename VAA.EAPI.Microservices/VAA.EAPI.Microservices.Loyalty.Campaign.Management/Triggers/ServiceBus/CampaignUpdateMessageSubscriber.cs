using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus
{
    public class CampaignUpdateMessageSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignUpdateMessageSubscriber> _logger;
        public CampaignUpdateMessageSubscriber(IMediator mediator, ILogger<CampaignUpdateMessageSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [Function("CampaignUpdateMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.CampaignUpdateEventHandlerTopicName, Constants.Application.CampaignUpdateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("CampaignUpdateEvent received");
            await _mediator.Send(Json.Deserialize<CampaignUpdateEventProcessCommand>(eventRequestPayload));
        }
    }
}
