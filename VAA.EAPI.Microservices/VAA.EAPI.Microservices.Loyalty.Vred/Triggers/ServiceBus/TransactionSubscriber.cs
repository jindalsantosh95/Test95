using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Events;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Triggers.ServiceBus
{
    public class TransactionSubscriber
    {
        private readonly IMediator _mediator;
        private readonly IUpdateTransactionPnrEventHandler _updateTransactionPnrEventHandler;

        public TransactionSubscriber(IMediator mediator, IUpdateTransactionPnrEventHandler updateTransactionPnrEventHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _updateTransactionPnrEventHandler = updateTransactionPnrEventHandler ?? throw new ArgumentNullException(nameof(updateTransactionPnrEventHandler));
        }

        [Function("CancelTransactionEventSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.ServiceBusTopicNameForVred, Constants.Application.CancelVredTransactionsEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<CancelTransactionEventProcessCommand>(eventRequestPayload));
        }

        [Function("CreateVredTransactionsSubscriber")]
        public async Task CreateVredTransactionsSubscriberAsync([ServiceBusTrigger(Constants.Application.ServiceBusTopicNameForVred, Constants.Application.CreateVredTransactionsSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<CreateTransactionEvent>(eventRequestPayload));
        }

        [Function("UpdateVredTransactionPnrSubscriber")]
        public async Task UpdateVredTransactionPnrSubscriber([ServiceBusTrigger(Constants.Application.ServiceBusTopicNameForVred, Constants.Application.TransactionPnrUpdateSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, CancellationToken cancellationToken)
        {
            await _updateTransactionPnrEventHandler.Handle(Json.Deserialize<UpdateTransactionPnrEvent>(eventRequestPayload), cancellationToken);
        }
    }
}
