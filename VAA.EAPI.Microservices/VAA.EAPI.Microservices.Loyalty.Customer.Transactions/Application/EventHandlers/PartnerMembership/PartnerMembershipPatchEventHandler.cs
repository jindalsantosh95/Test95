using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.PartnerMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.PartnerMembership
{
    public class PartnerMembershipPatchEventHandler : TransactionBaseEventHandler, IRequestHandler<PartnerMembershipPatchEvent, Result<Unit>>
    {
        public PartnerMembershipPatchEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        { }

        public async Task<Result<Unit>> Handle(PartnerMembershipPatchEvent request, CancellationToken cancellationToken)
        {
            var response = await base.Handle(request, cancellationToken);

            if (response.IsSuccess)
            {
                Domain.PartnerMembership.PartnerMembership partnerMembershipDocument = Json.DeserializeObject<PartnerMembershipCommandBase>((JToken)request.Data).PartnerMembershipDocument;
                var result = PartnerMembershipCommandBase.GetPartnerMembershipDoc(_mediator, partnerMembershipDocument.CustomerId, cancellationToken).Result;
                result = result.Where(n => n.Id != partnerMembershipDocument.Id && n.PartnerCode == partnerMembershipDocument.PartnerCode).Any(n => n.EndDate is null || n.EndDate.Value.Date > DateTime.UtcNow.Date) ? result : null;

                UpdatePartnerMembershipAuditDoc(partnerMembershipDocument);

                if (partnerMembershipDocument.EndDate is not null && !(partnerMembershipDocument.EndDate.Value.Date > DateTime.UtcNow.Date) && result == null)
                {
                    UpdateFCMDoc(partnerMembershipDocument);
                }
            }

            return response;
        }

        protected override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            PartnerMembershipCommandBase patchEvent = Json.DeserializeObject<PartnerMembershipCommandBase>((JToken)transactionEvent.Data);
            var validationResult = new PartnerMembershipPatchValidator().Validate(patchEvent.PartnerMembershipDocument);

            if (!validationResult.IsValid)
                throw new RequestValidationException(" PartnerMembership update request contains invalid data or missing mandatory fields.", validationResult.Errors);

            var existingDocument = PartnerMembershipCommandBase.GetPartnerMembershipDoc(_mediator, patchEvent.CustomerId, cancellationToken).Result.FirstOrDefault(n => n.Id == patchEvent.PartnerMembershipDocument.Id);

            if (existingDocument is null)
                throw new RequestValidationException(Constants.ErrorCodes.PartnerMembershipIdInvalid);

            // Update the changed fields
            existingDocument.LastUpdatedBy = patchEvent.PartnerMembershipDocument.LastUpdatedBy;
            existingDocument.EndDate = Convert.ToDateTime(patchEvent.PartnerMembershipDocument.EndDate?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat));

            PartnerMembershipDTO dto = (PartnerMembershipDTO)existingDocument;

            return Task.FromResult(new CosmosDocumentEvent(dto, transactionEvent.EventType));
        }

        private void UpdatePartnerMembershipAuditDoc(Domain.PartnerMembership.PartnerMembership partnerMembership)
        {
            CardLinkingAuditDTO auditDto = (CardLinkingAuditDTO)partnerMembership;
            auditDto.Id = Guid.NewGuid().ToString();

            PartnerMembershipAuditEvent auditUpdateEvent = new()
            {
                CardLinkingAudit = auditDto,
            };

            var cloudEvent = AuditPartnerMembershipEventToCloudEventMapper.CommandToEventMapper(auditUpdateEvent);

            PublishEvent(cloudEvent);
        }

        private void UpdateFCMDoc(Domain.PartnerMembership.PartnerMembership partnerMembership)
        {
            Domain.FlyingClubMember.FlyingClubMember fcMemberPatchdoc = new()
            {
                CustomerId = partnerMembership.CustomerId,
                LastUpdatedBy = partnerMembership.LastUpdatedBy,
                DowngradeProhibitFlag = Constants.Application.DisableDowngradeProhibitFlag,
                CenturianFlag = Constants.Application.DisableCenturionFlag
            };

            FlyingClubMemberCommandBase fcMemberPatchEvent = new()
            {
                FlyingClubMemberDocument = fcMemberPatchdoc,
                CustomerId = partnerMembership.CustomerId
            };

            var cloudEvent = FlyingClubMemberPatchCommandToEventMapper.CommandToEventMapper(fcMemberPatchEvent);

            cloudEvent.Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), TransactionCommandType.UpdateFCMForEditCenturionCardEvent.ToString()));

            PublishEvent(cloudEvent);
        }

        private void PublishEvent(EAPI.Domain.Events.CloudEvent cloudEvent)
        {
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
