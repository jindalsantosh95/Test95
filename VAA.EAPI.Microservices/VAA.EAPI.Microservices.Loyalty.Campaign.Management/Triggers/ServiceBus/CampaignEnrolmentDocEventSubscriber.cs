using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignEnrolmentDoc;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus
{
    public class CampaignEnrolmentDocEventSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignEnrolmentDocEventSubscriber> _logger;
        public CampaignEnrolmentDocEventSubscriber(IMediator mediator, ILogger<CampaignEnrolmentDocEventSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [Function("CampaignEnrolmentDocSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.CampaignUpdateEventHandlerTopicName, Constants.Application.CampaignEnrolmentDocEventSubscriberName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("CampaignEnrolmentDocEvent received");
            await _mediator.Send(Json.Deserialize<CampaignEnrolmentDocEventProcessCommand>(eventRequestPayload));
        }
    }
}
