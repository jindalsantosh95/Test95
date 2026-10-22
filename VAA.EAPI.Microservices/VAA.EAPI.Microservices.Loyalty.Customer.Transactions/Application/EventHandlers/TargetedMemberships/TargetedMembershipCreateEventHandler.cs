using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
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
    public class TargetedMembershipCreateEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public TargetedMembershipCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }
    public class TargetedMembershipCreateEventHandler : TransactionBaseEventHandler, IRequestHandler<TargetedMembershipCreateEvent, Result<Unit>>
    {
        public TargetedMembershipCreateEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        { }

        public Task<Result<Unit>> Handle(TargetedMembershipCreateEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected /*async*/ override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            TargetedMembershipCommandBase createEvent = Json.DeserializeObject<TargetedMembershipCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new TargetedMembershipCreateValidator().Validate(createEvent.TargetedMembershipDocument);
            if (!validationResult.IsValid)
                throw new RequestValidationException("Contact note create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            TargetedMembershipDTO dto = new(createEvent.TargetedMembershipDocument)
            {
                // Fill in the blanks (for a new record)
                CustomerId = createEvent.CustomerId,
                PartitionKey = createEvent.CustomerId,
                Id = transactionEvent.Id,
                TargetedMembershipId = transactionEvent.Id,
                CreatedBy = createEvent.TargetedMembershipDocument.CreatedBy,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = createEvent.TargetedMembershipDocument.CreatedBy,
                LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                ProfilingCampaignSourceId = createEvent.TargetedMembershipDocument.ProfilingCampaignSourceId,

            };

            return Task.FromResult(new CosmosDocumentEvent(dto, transactionEvent.EventType));
        }
    }


}
