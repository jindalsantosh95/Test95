using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Security.Applications.Events.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Triggers.ServiceBus
{
    internal class APIMSubscriptionEventSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<APIMSubscriptionEventSubscriber> _logger;
        public APIMSubscriptionEventSubscriber(IMediator mediator, ILogger<APIMSubscriptionEventSubscriber> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [Function("CreateSubscriptionMessageSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.APIMSubscriptionEventTopicName, Constants.Application.APIMSubscriptionCreateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var req = Json.Deserialize<SubscriptionCreateEventCommand>(eventRequestPayload);
            await _mediator.Send(req, CancellationToken.None);

        }
        [Function("UpdateSubscriptionMessageSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.APIMSubscriptionEventTopicName, Constants.Application.APIMSubscriptionUpdateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var req = Json.Deserialize<SubscriptionUpdateEventCommand>(eventRequestPayload);

            await _mediator.Send(req, CancellationToken.None);

        }
        [Function("DeleteSubscriptionMessageSubscriber")]
        public async Task Delete([ServiceBusTrigger(Constants.Application.APIMSubscriptionEventTopicName, Constants.Application.APIMSubscriptionDeleteEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var req = Json.Deserialize<SubscriptionDeleteEventCommand>(eventRequestPayload);
            await _mediator.Send(req, CancellationToken.None);

        }
    }
}

