using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.CampaignEnrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
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

        [Function("CampaignEnrolmentDocMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.CampaignEnrolmentDocEventTopicName, Constants.Application.CampaignEnrolmentDocEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("CampaignEnrolmentDocMessageSubscriber");
            log.LogInformation("CampaignEnrolmentDocMessageSubscriber");
            await _mediator.Send(Json.Deserialize<CampaignEnrolmentCreateEvent>(eventRequestPayload));
        }

    }
}
