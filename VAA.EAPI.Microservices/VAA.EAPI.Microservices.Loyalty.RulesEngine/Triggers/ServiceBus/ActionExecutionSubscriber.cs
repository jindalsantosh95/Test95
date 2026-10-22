using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Triggers.ServiceBus
{
    public class ActionExecutionSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ActionExecutionSubscriber> _logger;
        public ActionExecutionSubscriber(IMediator mediator, ILogger<ActionExecutionSubscriber> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("ActionExecutionSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.RuleEngineEventTopicName, Constants.Application.ActionExecutionSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            _logger.LogInformation("TierRuleSet Event received");
            await _mediator.Send(Json.Deserialize<ActionExecutionEventCommand>(eventRequestPayload));
        }
    }
}
