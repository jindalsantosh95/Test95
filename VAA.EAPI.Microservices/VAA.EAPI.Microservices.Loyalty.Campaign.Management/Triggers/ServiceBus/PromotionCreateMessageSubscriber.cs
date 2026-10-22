using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionCreate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus
{
    public class PromotionCreateMessageSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PromotionCreateMessageSubscriber> _logger;
        public PromotionCreateMessageSubscriber(IMediator mediator, ILogger<PromotionCreateMessageSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [Function("PromotionCreateMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.CampaignUpdateEventHandlerTopicName, Constants.Application.PromotionCreateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("PromotionCreateEvent received");
            await _mediator.Send(Json.Deserialize<PromotionCreateEventProcessCommand>(eventRequestPayload));
        }
    }
}
