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
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Awards
{
    public class AwardPatchEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public AwardPatchEvent()
        {
            EventType = CosmosModificationType.Replace;
        }
    }
    public class AwardPatchEventHandler : TransactionBaseEventHandler, IRequestHandler<AwardPatchEvent, Result<Unit>>
    {
        public AwardPatchEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        { }

        public async Task<Result<Unit>> Handle(AwardPatchEvent request, CancellationToken cancellationToken)
        {
            var response = await base.Handle(request, cancellationToken);

            if (response.IsSuccess == true)
            {
                Award award = Json.DeserializeObject<AwardCommandBase>((JToken)request.Data).Award;
                var existingAward = (await AwardCommandBase.GetListOfAwards(_mediator, award.CustomerId, cancellationToken))
                                                            .FirstOrDefault(n => n.AwardId == award.AwardId);

                existingAward.AwardName = award.AwardName;
                PatchAwardAuditDoc(existingAward);
            }

            return response;
        }

        protected /*async*/ override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            AwardCommandBase transactionEventData = Json.DeserializeObject<AwardCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new AwardPatchValidator().Validate(transactionEventData.Award);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Award update request contains invalid data or missing mandatory fields.", validationResult.Errors);

            var existingDocument = AwardCommandBase.GetListOfAwards(_mediator, transactionEventData.Award.CustomerId, cancellationToken).Result.FirstOrDefault(x => x.Id == transactionEventData.Award.Id);

            if (existingDocument is null)
                throw new RequestValidationException(Constants.ErrorCodes.AwardIdInvalid);

            AwardDTO dto = new(existingDocument)
            {
                // Update the changed fields
                LastUpdatedBy = transactionEventData.Award.LastUpdatedBy ?? transactionEvent.Originator,
                LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                ExpiryDate = transactionEventData.Award.ExpiryDate?.ToString(Constants.Application.ISODateTimeFormat) ?? existingDocument.ExpiryDate?.ToString(Constants.Application.ISODateTimeFormat),
                Status = transactionEventData.Award.Status ?? existingDocument.Status,
                TransactionId = transactionEventData.Award.TransactionId ?? existingDocument.TransactionId,
                UsedDate = transactionEventData.Award.UsedDate?.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty,
                Quantity = transactionEventData.Award.Quantity.ToString(),
                AwardValue = transactionEventData.Award.AwardValue.ToString(),
                AwardBasis = transactionEventData.Award.AwardBasis ?? string.Empty,
                ExpiryDateUpdatedBy = transactionEventData.Award.ExpiryDateUpdatedBy ?? string.Empty,
                GiftedAwardDate = transactionEventData.Award.GiftedAwardDate?.ToString(Constants.Application.ISODateTimeFormat) ?? existingDocument.GiftedAwardDate?.ToString(Constants.Application.ISODateTimeFormat),
                GiftedFromCustomerId = transactionEventData.Award.GiftedFromCustomerId ?? existingDocument.GiftedFromCustomerId,
                GiftedToCustomerId = transactionEventData.Award.GiftedToCustomerId ?? existingDocument.GiftedToCustomerId,
                PreviousStatus = transactionEventData.Award.PreviousStatus ?? existingDocument.PreviousStatus
            };

            return Task.FromResult(new CosmosDocumentEvent(dto, transactionEvent.EventType));
        }

        private void PatchAwardAuditDoc(Award award)
        {
            AwardAuditDTO auditDto = (AwardAuditDTO)award;
            auditDto.Id = Guid.NewGuid().ToString();
            auditDto.AwardName = award.AwardName;

            AwardAuditEvent awardAuditEvent = new AwardAuditEvent()
            {
                awardAudit = auditDto
            };

            var cloudEvent = AuditAwardEventToCloudEventMapper.CommandToEventMapper(awardAuditEvent);

            try
            {
                _mediator.Send(new EventRequestInsertCommand
                {
                    Id = cloudEvent.Id,
                    Type = cloudEvent.Type,
                    Data = cloudEvent,
                    Subject = cloudEvent.Subject
                });

                _mediator.Publish(cloudEvent);
            }
            catch
            {
                _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                throw;
            }
        }
    }
}
