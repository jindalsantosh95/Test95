using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.MembershipChange;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.MembershipChange
{
    public class MembershipChangeCreateEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public MembershipChangeCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }

    public class MembershipChangeCreateEventHandler : TransactionBaseEventHandler, IRequestHandler<MembershipChangeCreateEvent, Result<Unit>>
    {
        public MembershipChangeCreateEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        { }

        public Task<Result<Unit>> Handle(MembershipChangeCreateEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            MembershipChangeCommandBase createEvent = Json.DeserializeObject<MembershipChangeCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new MembershipChangeCreateValidator().Validate(createEvent.MembershipChangeDocument);
            if (!validationResult.IsValid)
                throw new RequestValidationException("Membership Change create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            IEnumerable<Domain.MembershipChange.MembershipChange> existingDoc = await MembershipChangeCommandBase.GetMembershipChangeDocs(_mediator, createEvent.MembershipChangeDocument.CustomerId, cancellationToken);

            MembershipChangeDTO dto = new(createEvent.MembershipChangeDocument)
            {
                // Fill in the blanks (for a new record)
                Active = createEvent.MembershipChangeDocument.Active,
                CustomerId = createEvent.MembershipChangeDocument.CustomerId,
                PartitionKey = createEvent.MembershipChangeDocument.CustomerId,
                Id = transactionEvent.Id,
                CreatedBy = createEvent.MembershipChangeDocument.CreatedBy,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = createEvent.MembershipChangeDocument.LastUpdatedBy,
                LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                MemberNumber = createEvent.MembershipChangeDocument.MemberNumber,
                ApprovalDate = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                EndDate = createEvent.MembershipChangeDocument.EndDate?.ToString(Constants.Application.ISODateTimeFormat),
                StartDate = createEvent.MembershipChangeDocument.StartDate?.ToString(Constants.Application.ISODateTimeFormat),
                Revision = (existingDoc.Count() + 1).ToString(),
            };

            return new CosmosDocumentEvent(dto, transactionEvent.EventType);
        }
    }
}
