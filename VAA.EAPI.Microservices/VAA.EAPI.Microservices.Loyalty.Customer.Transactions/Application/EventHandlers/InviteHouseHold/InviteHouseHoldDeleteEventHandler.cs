using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.InviteHouseHold
{
    public class InviteHouseHoldDeleteEventHandler : TransactionBaseEventHandler, IRequestHandler<InviteHouseHoldDeleteEvent, Result<Unit>>
    {
        public InviteHouseHoldDeleteEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        {
        }

        public Task<Result<Unit>> Handle(InviteHouseHoldDeleteEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            try
            {
                var deleteEvent = Json.DeserializeObject<InviteHouseHoldRepoQuery>((JToken)transactionEvent.Data);

                var inviteDocs = await _mediator.Send(new InviteHouseHoldRepoQuery(deleteEvent.InviteeMemberNumber) { }, cancellationToken);

                InviteHouseHoldDTO dto = JsonConvert.DeserializeObject<List<InviteHouseHoldDTO>>(JsonConvert.SerializeObject(inviteDocs)).First(x => x.InviteeMemberNumber == deleteEvent.InviteeMemberNumber);

                return new CosmosDocumentEvent(dto, transactionEvent.EventType);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured while deleting household invite document.");
                throw;
            }
        }
    }
}
