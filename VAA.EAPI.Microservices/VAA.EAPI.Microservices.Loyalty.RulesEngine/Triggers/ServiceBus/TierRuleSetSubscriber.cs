using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Triggers.ServiceBus
{
    public class TierRuleSetSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TierRuleSetSubscriber> _logger;
        public TierRuleSetSubscriber(IMediator mediator, ILogger<TierRuleSetSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [Function("TierRuleSetSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.RuleEngineEventTopicName, Constants.Application.TierRuleSetEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            _logger.LogInformation("TierRuleSet Event received");
            await _mediator.Send(Json.Deserialize<TierRuleSetEventCommand>(eventRequestPayload));
        }
    }
}
