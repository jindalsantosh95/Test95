using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignCreate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus
{
    public class CampaignCreateMessageSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignCreateMessageSubscriber> _logger;
        public CampaignCreateMessageSubscriber(IMediator mediator, ILogger<CampaignCreateMessageSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("CampaignCreateMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.CampaignManagementEventTopicName, Constants.Application.CampaignCreateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("CampaignCreateEvent received");
            await _mediator.Send(Json.Deserialize<CampaignCreateEventProcessCommand>(eventRequestPayload));
        }
    }
}
