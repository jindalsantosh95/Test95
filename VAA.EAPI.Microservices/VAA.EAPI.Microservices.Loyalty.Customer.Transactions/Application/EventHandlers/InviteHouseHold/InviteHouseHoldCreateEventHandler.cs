using System.Threading;
using System.Threading.Tasks;

using CSharpFunctionalExtensions;

using MediatR;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.InviteHouseHold
{
    public class InviteHouseHoldCreateEventHandler : TransactionBaseEventHandler, IRequestHandler<InviteHouseHoldCreateEvent, Result<Unit>>
    {
        public InviteHouseHoldCreateEventHandler(IMediator mediator,
            ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        {

        }

        public Task<Result<Unit>> Handle(InviteHouseHoldCreateEvent request,
           CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected /*async*/ override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            Inviter Inviter = Json.DeserializeObject<Inviter>((JToken)transactionEvent.Data);

            return Inviter == null
                ? throw new RequestValidationException("Invalid Request in InviteHouseHold CreateEvent Handler")
                : Task.FromResult(new CosmosDocumentEvent((InviteHouseHoldDTO)Inviter, transactionEvent.EventType));
        }
    }
}
