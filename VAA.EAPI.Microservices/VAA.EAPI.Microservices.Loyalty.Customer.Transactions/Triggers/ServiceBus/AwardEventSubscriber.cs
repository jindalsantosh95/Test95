using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Awards;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class AwardEventSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AwardEventSubscriber> _logger;

        public AwardEventSubscriber(IMediator mediator, ILogger<AwardEventSubscriber> logger)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [Function("AssignTopMemberMessageSubscriber")]
        public async Task TopMember([ServiceBusTrigger(Constants.Application.AwardEventTopicName, Constants.Application.AssignTopMemberEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("AwardEventSubscriber");
            log.LogInformation("Award Create Event Service Bus Request");
            await _mediator.Send(Json.Deserialize<TopMemberEvent>(eventRequestPayload));
        }

        [Function("CreateAwardMessageSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.AwardEventTopicName, Constants.Application.AwardCreateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("AwardEventSubscriber");
            log.LogInformation("Award Create Event Service Bus Request");
            await _mediator.Send(Json.Deserialize<AwardCreateEvent>(eventRequestPayload));
        }

        [Function("DeleteAwardMessageSubscriber")]
        public async Task Delete([ServiceBusTrigger(Constants.Application.AwardEventTopicName, Constants.Application.AwardDeleteEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("AwardEventSubscriber");
            log.LogInformation("Award Delete Event Service Bus Request");
            await _mediator.Send(Json.Deserialize<AwardDeleteEvent>(eventRequestPayload));
        }

        [Function("UpdateAwardMessageSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.AwardEventTopicName, Constants.Application.AwardUpdateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("AwardEventSubscriber");
            log.LogInformation("Award Update Event Service Bus Request");
            await _mediator.Send(Json.Deserialize<AwardPatchEvent>(eventRequestPayload));
        }

    }
}
