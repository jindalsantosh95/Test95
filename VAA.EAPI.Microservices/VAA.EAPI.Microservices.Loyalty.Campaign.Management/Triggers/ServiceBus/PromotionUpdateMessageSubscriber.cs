using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.PromotionUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Triggers.ServiceBus
{
    public class PromotionUpdateMessageSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PromotionUpdateMessageSubscriber> _logger;
        public PromotionUpdateMessageSubscriber(IMediator mediator, ILogger<PromotionUpdateMessageSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [Function("PromotionUpdateMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.CampaignUpdateEventHandlerTopicName, Constants.Application.PromotionUpdateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("CampaignUpdateEvent received");
            await _mediator.Send(Json.Deserialize<PromotionUpdateEventProcessCommand>(eventRequestPayload));
        }
    }
}
