using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.GroupDoc;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class GroupDocEventSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GroupDocEventSubscriber> _logger;

        public GroupDocEventSubscriber(IMediator mediator, ILogger<GroupDocEventSubscriber> logger)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [Function("UpdateGroupDocumentEventSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.FlyingClubMemberEventTopicName, Constants.Application.GroupDocUpdateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("UpdateGroupDocumentEventSubscriber - Event Started");
            try
            {
                await _mediator.Send(Json.Deserialize<GroupDocPatchEvent>(eventRequestPayload));
            }
            finally
            {
                _logger.LogInformation("UpdateGroupDocumentEventSubscriber - Event ended");
            }
        }

        [Function("CreateGroupDocumentEventSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.FlyingClubMemberEventTopicName, Constants.Application.GroupDocCreateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("CreateGroupDocumentEventSubscriber - Event started");
            try
            {
                await _mediator.Send(Json.Deserialize<GroupDocCreateEvent>(eventRequestPayload));
            }
            finally
            {
                _logger.LogInformation("CreateGroupDocumentEventSubscriber - Event ended");
            }
        }
    }
}
