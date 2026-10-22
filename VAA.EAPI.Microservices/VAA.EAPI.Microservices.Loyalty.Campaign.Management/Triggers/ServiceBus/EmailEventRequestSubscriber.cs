using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignRegister;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus
{
    public class EmailEventRequestSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EmailEventRequestSubscriber> _logger;
        public EmailEventRequestSubscriber(IMediator mediator, ILogger<EmailEventRequestSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("EmailEventSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.CampaignManagementEventTopicName, Constants.Application.EmailEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            _logger.LogInformation("Event received for sending  Email");
            await _mediator.Send(Json.Deserialize<EmailEventRequestProcessCommand>(eventRequestPayload));
        }
    }
}
