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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.CampaignEnrolmentDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.CampaignEnrolment
{
    public class CampaignEnrolmentCreateEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public CampaignEnrolmentCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }
    public class CampaignEnrolmentCreateEventHandler : TransactionBaseEventHandler, IRequestHandler<CampaignEnrolmentCreateEvent, Result<Unit>>
    {
        public CampaignEnrolmentCreateEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        { }

        public Task<Result<Unit>> Handle(CampaignEnrolmentCreateEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected /*async*/ override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            CampaignEnrolmentCommandBase createEvent = Json.DeserializeObject<CampaignEnrolmentCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new CampaignEnrolmentCreateValidator().Validate(createEvent.CampaignEnrolmentDocument);
            if (!validationResult.IsValid)
                throw new RequestValidationException("Contact note create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            CampaignEnrolmentDTO dto = new(createEvent.CampaignEnrolmentDocument)
            {
                // Fill in the blanks (for a new record)
                CustomerId = createEvent.CampaignEnrolmentDocument.CustomerId,
                PartitionKey = createEvent.CampaignEnrolmentDocument.CustomerId,
                Id = transactionEvent.Id,
                EnrollmentId = transactionEvent.Id,
                PromotionId = createEvent.CampaignEnrolmentDocument.PromotionId,
                CreatedBy = createEvent.CampaignEnrolmentDocument.CreatedBy,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = createEvent.CampaignEnrolmentDocument.CreatedBy,
                LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                MemberNumber = createEvent.CampaignEnrolmentDocument.MemberNumber,
                OptInDate = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat)
            };

            return Task.FromResult(new CosmosDocumentEvent(dto, transactionEvent.EventType));
        }
    }


}
