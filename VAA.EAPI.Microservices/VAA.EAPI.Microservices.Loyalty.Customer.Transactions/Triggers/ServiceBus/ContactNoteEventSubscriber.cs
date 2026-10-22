using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.ContactNotes;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class ContactNoteEventSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ContactNoteEventSubscriber> _logger;

        public ContactNoteEventSubscriber(IMediator mediator, ILogger<ContactNoteEventSubscriber> logger)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [Function("ContactNoteMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.ContactNoteEventTopicName, Constants.Application.ContactNoteCreateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("ContactNoteMessageSubscriber");
            log.LogInformation("ContactNoteMessageSubscriber");
            await _mediator.Send(Json.Deserialize<ContactNoteCreateEvent>(eventRequestPayload));
        }

        [Function("DeleteContactNoteMessageSubscriber")]
        public async Task Delete([ServiceBusTrigger(Constants.Application.ContactNoteEventTopicName, Constants.Application.ContactNoteDeleteEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("ContactNoteMessageSubscriber");
            log.LogInformation("DeleteContactNoteMessageSubscriber");
            await _mediator.Send(Json.Deserialize<ContactNoteDeleteEvent>(eventRequestPayload));
        }

        [Function("UpdateContactNoteMessageSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.ContactNoteEventTopicName, Constants.Application.ContactNoteUpdateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("ContactNoteMessageSubscriber");
            log.LogInformation("UpdateContactNoteMessageSubscriber");
            await _mediator.Send(Json.Deserialize<ContactNotePatchEvent>(eventRequestPayload));
        }

    }
}
