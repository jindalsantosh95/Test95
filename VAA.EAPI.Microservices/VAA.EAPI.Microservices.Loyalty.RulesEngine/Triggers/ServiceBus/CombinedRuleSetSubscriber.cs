using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Triggers.ServiceBus
{
    public class CombinedRuleSetSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CombinedRuleSetSubscriber> _logger;

        public CombinedRuleSetSubscriber(IMediator mediator, ILogger<CombinedRuleSetSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("CombinedRuleSetSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.RuleEngineEventTopicName, Constants.Application.CombinedRuleSetEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            _logger.LogInformation("CombinedRuleSet Event received");
            await _mediator.Send(Json.Deserialize<CombinedRuleSetEventCommmand>(eventRequestPayload));
        }
    }
}
