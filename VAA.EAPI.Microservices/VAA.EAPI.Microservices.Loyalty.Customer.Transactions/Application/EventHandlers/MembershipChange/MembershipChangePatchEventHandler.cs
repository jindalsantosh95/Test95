using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.MembershipChange
{
    public class MembershipChangePatchEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public MembershipChangePatchEvent()
        {
            EventType = CosmosModificationType.Replace;
        }
    }

    public class MembershipChangePatchEventHandler : TransactionBaseEventHandler, IRequestHandler<MembershipChangePatchEvent, Result<Unit>>
    {
        public MembershipChangePatchEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        { }

        public Task<Result<Unit>> Handle(MembershipChangePatchEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            MembershipChangeCommandBase patchEvent = Json.DeserializeObject<MembershipChangeCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new MembershipChangePatchValidator().Validate(patchEvent.MembershipChangeDocument);
            if (!validationResult.IsValid)
                throw new RequestValidationException("Membership Change Patch request contains invalid data or missing mandatory fields.", validationResult.Errors);

            var existingDoc = (await MembershipChangeCommandBase.GetMembershipChangeDocs(_mediator, patchEvent.MembershipChangeDocument.CustomerId, cancellationToken)).LastOrDefault(n => n.Active == "Y");

            if (existingDoc is null)
                throw new RequestValidationException(ErrorCodes.ContactNoteIdInvalid);

            MembershipChangeDTO dto = new(existingDoc)
            {
                Active = patchEvent.MembershipChangeDocument.Active,
                EndDate = patchEvent.MembershipChangeDocument.EndDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = patchEvent.MembershipChangeDocument.LastUpdatedBy,
                LastUpdatedOn = DateTime.Now.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
            };
            return new CosmosDocumentEvent(dto, transactionEvent.EventType);
        }
    }
}
