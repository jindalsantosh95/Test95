using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class TransactionEventSubscriber
    {
        private readonly IMediator _mediator;

        public TransactionEventSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [Function("CreateTransactionEventSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.TransactionEventTopicName, Constants.Application.TransactionCreateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            await _mediator.Send(Json.Deserialize<TransactionCreateEvent>(eventRequestPayload));
        }

        [Function("CancelTransactionEventSubscriber")]
        public async Task Cancel([ServiceBusTrigger(Constants.Application.TransactionEventTopicName, Constants.Application.TransactionCancelEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            await _mediator.Send(Json.Deserialize<TransactionCancelEvent>(eventRequestPayload));
        }

        [Function("UpdateTransactionEventSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.TransactionEventTopicName, Constants.Application.TransactionUpdateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            await _mediator.Send(Json.Deserialize<TransactionUpdateEvent>(eventRequestPayload));
        }
    }
}
