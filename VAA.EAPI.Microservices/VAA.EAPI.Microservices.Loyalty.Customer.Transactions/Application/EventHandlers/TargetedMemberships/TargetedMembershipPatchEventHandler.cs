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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.TargetedMemberships;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.TargetedMemberships
{
    public class TargetedMembershipPatchEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public TargetedMembershipPatchEvent()
        {
            EventType = CosmosModificationType.Replace;
        }
    }
    public class TargetedMembershipPatchEventHandler : TransactionBaseEventHandler, IRequestHandler<TargetedMembershipPatchEvent, Result<Unit>>
    {
        public TargetedMembershipPatchEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        {
        }

        public Task<Result<Unit>> Handle(TargetedMembershipPatchEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            TargetedMembershipCommandBase patchEvent = Json.DeserializeObject<TargetedMembershipCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new TargetedMembershipDeleteValidator().Validate(patchEvent.TargetedMembershipDocument);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Contact note update request contains invalid data or missing mandatory fields.", validationResult.Errors);

            var existingDoc = (await TargetedMembershipCommandBase.GetListOfTargetedMemberships(_mediator, patchEvent.CustomerId, cancellationToken))
                                                        .FirstOrDefault(n => n.Id == patchEvent.TargetedMembershipDocument.Id);
            if (existingDoc is null)
                throw new RequestValidationException(Constants.ErrorCodes.TargetedMembershipIdInvalid);


            TargetedMembershipDTO dto = new(existingDoc)
            {
                // Update the changed fields
                AssignmentDate = patchEvent.TargetedMembershipDocument.AssignmentDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat) ?? existingDoc.AssignmentDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat),
                Comments = patchEvent.TargetedMembershipDocument.Comments ?? existingDoc.Comments,
                CompletedDate = patchEvent.TargetedMembershipDocument.CompletedDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat) ?? existingDoc.CompletedDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat),
                ContactedDate = patchEvent.TargetedMembershipDocument.ContactedDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat) ?? existingDoc.ContactedDate?.ToUniversalTime().Date.ToString(Constants.Application.ISODateTimeFormat),
                ProfilingCampaignSourceId = patchEvent.TargetedMembershipDocument.ProfilingCampaignSourceId,
                StatusCode = patchEvent.TargetedMembershipDocument.StatusCode,
                LastUpdatedBy = patchEvent.TargetedMembershipDocument.CreatedBy,
                LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
            };

            return new CosmosDocumentEvent(dto, transactionEvent.EventType);
        }
    }
}
