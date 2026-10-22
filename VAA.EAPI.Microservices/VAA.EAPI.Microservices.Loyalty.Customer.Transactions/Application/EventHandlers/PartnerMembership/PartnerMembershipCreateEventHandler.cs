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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.PartnerMembership
{
    public class PartnerMembershipCreateEventHandler : TransactionBaseEventHandler, IRequestHandler<PartnerMembershipCreateEvent, Result<Unit>>
    {
        public PartnerMembershipCreateEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        { }

        public async Task<Result<Unit>> Handle(PartnerMembershipCreateEvent request, CancellationToken cancellationToken)
        {
            var response = await base.Handle(request, cancellationToken);

            if (response.IsSuccess == true)
            {
                Domain.PartnerMembership.PartnerMembership partnerMembership = Json.DeserializeObject<PartnerMembershipCommandBase>((JToken)request.Data).PartnerMembershipDocument;
                partnerMembership.Id = request.Id;

                CreatePartnerMembershipAuditDoc(partnerMembership);
                UpdateFCMDoc(partnerMembership.CustomerId, partnerMembership.CreatedBy);
            }

            return response;
        }

        protected override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            PartnerMembershipCommandBase createEvent = Json.DeserializeObject<PartnerMembershipCommandBase>((JToken)transactionEvent.Data);
            var result = PartnerMembershipCommandBase.GetPartnerMembershipDoc(_mediator, createEvent.CustomerId, cancellationToken).Result;

            if (result != null && result.Any(n => n.PartnerId == createEvent.PartnerMembershipDocument.PartnerId && n.PartnerStatusCode == createEvent.PartnerMembershipDocument.PartnerStatusCode && (n.EndDate is null || n.EndDate > DateTime.UtcNow)))
            {
                throw new RequestValidationException(Constants.ErrorCodes.DuplicateCardFound);
            }

            var validationResult = new PartnerMembershipCreateValidator().Validate(createEvent.PartnerMembershipDocument);

            if (!validationResult.IsValid)
                throw new RequestValidationException(" PartnerMembership create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            PartnerMembershipDTO partnerMembershipDto = (PartnerMembershipDTO)createEvent.PartnerMembershipDocument;
            partnerMembershipDto.Id = transactionEvent.Id;

            return Task.FromResult(new CosmosDocumentEvent(partnerMembershipDto, transactionEvent.EventType));
        }

        private void CreatePartnerMembershipAuditDoc(Domain.PartnerMembership.PartnerMembership partnerMembership)
        {
            CardLinkingAuditDTO auditDto = (CardLinkingAuditDTO)partnerMembership;
            auditDto.Id = Guid.NewGuid().ToString();

            PartnerMembershipAuditEvent auditCreateEvent = new PartnerMembershipAuditEvent()
            {
                CardLinkingAudit = auditDto,
            };

            var cloudEvent = AuditPartnerMembershipEventToCloudEventMapper.CommandToEventMapper(auditCreateEvent);

            PublishEvent(cloudEvent);
        }

        private void UpdateFCMDoc(string customerId, string initiatedBy)
        {
            Domain.FlyingClubMember.FlyingClubMember fcMemberPatchdoc = new Domain.FlyingClubMember.FlyingClubMember()
            {
                CustomerId = customerId,
                LastUpdatedBy = initiatedBy,
                CenturianFlag = Constants.Application.CenturianFlag
            };

            FlyingClubMemberCommandBase fcMemberPatchEvent = new FlyingClubMemberCommandBase()
            {
                FlyingClubMemberDocument = fcMemberPatchdoc,
                CustomerId = customerId
            };

            var cloudEvent = FlyingClubMemberPatchCommandToEventMapper.CommandToEventMapper(fcMemberPatchEvent);

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


