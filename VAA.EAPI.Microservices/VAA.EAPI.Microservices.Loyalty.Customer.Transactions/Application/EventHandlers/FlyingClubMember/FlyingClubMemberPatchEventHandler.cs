using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.FlyingClubMember
{
    public class FlyingClubMemberPatchEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public FlyingClubMemberPatchEvent()
        {
            EventType = CosmosModificationType.Replace;
        }
    }
    public class FlyingClubMemberPatchEventHandler : TransactionBaseEventHandler, IRequestHandler<FlyingClubMemberPatchEvent, Result<Unit>>
    {
        public FlyingClubMemberPatchEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        {
        }

        public Task<Result<Unit>> Handle(FlyingClubMemberPatchEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            FlyingClubMemberCommandBase patchEvent = Json.DeserializeObject<FlyingClubMemberCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new FlyingClubMemberPatchValidators().Validate(patchEvent.FlyingClubMemberDocument);

            if (!validationResult.IsValid)
                throw new RequestValidationException("FlyingClubMember update request contains invalid data or missing mandatory fields.", validationResult.Errors);
            FlyingClubMemberDTO dto = new();
            var existingDoc = (await FlyingClubMemberCommandBase.GetListOfFlyingClubMembers(_mediator, patchEvent.CustomerId, cancellationToken))
                                                        .FirstOrDefault(n => n.CustomerId == patchEvent.CustomerId);
            if (existingDoc is null)
                throw new RequestValidationException(ErrorCodes.CustomerIdNotFoundMessage);

            bool auditData = false;

            if ((!string.IsNullOrWhiteSpace(patchEvent.FlyingClubMemberDocument.DowngradeProhibitFlag) && patchEvent.FlyingClubMemberDocument.DowngradeProhibitFlag != existingDoc.DowngradeProhibitFlag) ||
                (!string.IsNullOrWhiteSpace(patchEvent.FlyingClubMemberDocument.GoldStartDate.ToString()) && patchEvent.FlyingClubMemberDocument.GoldStartDate != existingDoc.GoldStartDate))
            {
                auditData = true;
            }

            //update fields - FCM Doc
            if (patchEvent.FlyingClubMemberDocument.MemberAttributes?.Count > 0)
            {
                foreach (Domain.FlyingClubMember.MemberAttribute memberAttribute in patchEvent.FlyingClubMemberDocument.MemberAttributes)
                {
                    if (existingDoc.MemberAttributes.Any(x => x.AttributeName.Equals(memberAttribute.AttributeName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        Domain.FlyingClubMember.MemberAttribute memberAttributeUpsert = existingDoc.MemberAttributes?.Where(x => x.AttributeName.ToUpper() == patchEvent.FlyingClubMemberDocument.MemberAttributes.FirstOrDefault().AttributeName.ToUpper()).FirstOrDefault();
                        memberAttributeUpsert.Value = patchEvent.FlyingClubMemberDocument.MemberAttributes?.FirstOrDefault().Value;
                        memberAttributeUpsert.LastUpdatedOn = DateTime.UtcNow;
                        memberAttributeUpsert.LastUpdatedBy = patchEvent.FlyingClubMemberDocument.LastUpdatedBy;
                    }
                    else
                    {
                        Domain.FlyingClubMember.MemberAttribute newAttribute = new()
                        {
                            LastUpdatedBy = patchEvent.FlyingClubMemberDocument.LastUpdatedBy,
                            LastUpdatedOn = DateTime.UtcNow,
                            CreatedBy = patchEvent.FlyingClubMemberDocument.LastUpdatedBy,
                            CreatedOn = DateTime.UtcNow,
                            AttributeId = Utility.GetId(),
                            SourceIdentifier = string.Empty,
                            Value = patchEvent.FlyingClubMemberDocument.MemberAttributes?.FirstOrDefault().Value,
                            AttributeName = patchEvent.FlyingClubMemberDocument.MemberAttributes?.FirstOrDefault().AttributeName.ToUpper(),
                            SourceSystem = string.Empty,
                            MemberNumber = existingDoc.MemberNumber,
                        };
                        existingDoc.MemberAttributes.Add(newAttribute);
                    }
                }
            }
            existingDoc.FastTrackId = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.FastTrackId) ? patchEvent.FlyingClubMemberDocument.FastTrackId : existingDoc.FastTrackId;
            existingDoc.InstantSilverCampaignId = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.InstantSilverCampaignId) ? patchEvent.FlyingClubMemberDocument.InstantSilverCampaignId : existingDoc.InstantSilverCampaignId;
            existingDoc.VAInstantGoldCampaignId = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.VAInstantGoldCampaignId) ? patchEvent.FlyingClubMemberDocument.VAInstantGoldCampaignId : existingDoc.VAInstantGoldCampaignId;
            existingDoc.LastUpdatedOn = DateTime.UtcNow;

            //Set Tier FCM Update
            existingDoc.LastUpdatedBy = patchEvent.FlyingClubMemberDocument.LastUpdatedBy;
            existingDoc.GoldStartDate = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.GoldStartDate.ToString()) ? patchEvent.FlyingClubMemberDocument.GoldStartDate : existingDoc.GoldStartDate;
            if (patchEvent.FlyingClubMemberDocument?.MemberShipCardDetails is not null)
            {
                existingDoc.MemberShipCardDetails.Active = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.Active) ? patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.Active : existingDoc.MemberShipCardDetails?.Active;
                existingDoc.MemberShipCardDetails.CardStatusCode = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.CardStatusCode) ? patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.CardStatusCode : existingDoc.MemberShipCardDetails?.CardStatusCode;
                existingDoc.MemberShipCardDetails.CardType = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.CardType) ? patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.CardType : existingDoc.MemberShipCardDetails?.CardType;
                existingDoc.MemberShipCardDetails.TierCode = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.TierCode) ? patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.TierCode : existingDoc.MemberShipCardDetails?.TierCode;
                existingDoc.MemberShipCardDetails.RequestedDate = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.RequestedDate?.ToString()) ? patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.RequestedDate : existingDoc.MemberShipCardDetails?.RequestedDate;
                existingDoc.MemberShipCardDetails.IssueDate = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.IssueDate?.ToString()) ? patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.IssueDate : existingDoc.MemberShipCardDetails?.IssueDate;
                existingDoc.MemberShipCardDetails.StartDate = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.StartDate?.ToString()) ? patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.StartDate : existingDoc.MemberShipCardDetails?.StartDate;
                existingDoc.MemberShipCardDetails.ExpiryDate = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.ExpiryDate?.ToString()) ? patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.ExpiryDate : existingDoc.MemberShipCardDetails?.ExpiryDate;
                existingDoc.MemberShipCardDetails.CardExpiryDate = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.CardExpiryDate?.ToString()) ? patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.CardExpiryDate : existingDoc.MemberShipCardDetails?.CardExpiryDate;
                if (!string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.TierCode))
                {
                    existingDoc.MemberShipCardDetails.CardReplacementReason = existingDoc.TierCode == patchEvent.FlyingClubMemberDocument.TierCode
                                ? Constants.Application.RetainReason
                                : existingDoc.TierCode == Constants.Application.RedTier && (patchEvent.FlyingClubMemberDocument.TierCode == Constants.Application.GoldTier || patchEvent.FlyingClubMemberDocument.TierCode == Constants.Application.SilverTier) ||
                                  existingDoc.TierCode == Constants.Application.SilverTier && patchEvent.FlyingClubMemberDocument.TierCode == Constants.Application.GoldTier
                                ? Constants.Application.UpgradeReason
                                : Constants.Application.DowngradeReason;
                }
                existingDoc.MemberShipCardDetails.LastUpdatedBy = patchEvent.FlyingClubMemberDocument.MemberShipCardDetails?.LastUpdatedBy;
                existingDoc.MemberShipCardDetails.LastUpdatedOn = DateTime.UtcNow;
            }

            existingDoc.TierCode = !string.IsNullOrWhiteSpace(patchEvent.FlyingClubMemberDocument.TierCode) ? patchEvent.FlyingClubMemberDocument.TierCode : existingDoc.TierCode;
            existingDoc.GroupId = patchEvent.FlyingClubMemberDocument.GroupId ?? existingDoc.GroupId;
            existingDoc.HeadOfGroupMemberNumber = patchEvent.FlyingClubMemberDocument.HeadOfGroupMemberNumber ?? existingDoc.HeadOfGroupMemberNumber;
            existingDoc.MemberType = patchEvent.FlyingClubMemberDocument.MemberType ?? existingDoc.MemberType;
            existingDoc.SSAMemberNumber = patchEvent.FlyingClubMemberDocument.SSAMemberNumber ?? existingDoc.SSAMemberNumber;
            existingDoc.CenturianFlag = !string.IsNullOrWhiteSpace(patchEvent.FlyingClubMemberDocument.CenturianFlag) ? patchEvent.FlyingClubMemberDocument.CenturianFlag : existingDoc.CenturianFlag;
            existingDoc.Status = patchEvent.FlyingClubMemberDocument.Status ?? existingDoc.Status;
            existingDoc.MergeToMemberNumber = patchEvent.FlyingClubMemberDocument.MergeToMemberNumber ?? existingDoc.MergeToMemberNumber;

            if (transactionEvent.Subject == Constants.Application.UpdateFCMEventSubject)
            {
                //Post Event To Rule Engine
                TierRuleSetEntity tierRuleSetEntity = TierRuleSetEventMapper.MapTierRuleSetEventData(existingDoc, patchEvent);
                var cloudEvent = TierRuleSetEventMapper.MapTierRuleSetEvent(tierRuleSetEntity);
                await PublishEvent(cloudEvent);
            }

            if (auditData)
            {
                CustomerProfileAuditEvent customerProfileAuditEvent = new()
                {
                    CustomerProfileAuditDocument = new CustomerProfileAuditDTO
                    {
                        CustomerId = patchEvent.FlyingClubMemberDocument.CustomerId,
                        LoyaltyMemberNumber = existingDoc.MemberNumber,
                        CustomerProfileUpdatedBy = patchEvent.FlyingClubMemberDocument.LastUpdatedBy,
                        DowngradeProhibitFlag = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.DowngradeProhibitFlag) ? patchEvent.FlyingClubMemberDocument.DowngradeProhibitFlag : (string.IsNullOrEmpty(existingDoc.DowngradeProhibitFlag) ? "N" : existingDoc.DowngradeProhibitFlag),
                        TelephonePassword = existingDoc.TelephonePassword,
                        GoldStartDate = !string.IsNullOrEmpty(patchEvent.FlyingClubMemberDocument.GoldStartDate.ToString()) ? patchEvent.FlyingClubMemberDocument.GoldStartDate : existingDoc.GoldStartDate
                    }
                };

                var cloudEvent = CustomerProfileCommandToEventMapper.CommandToEventMapper(customerProfileAuditEvent);
                await PublishEvent(cloudEvent);
            }

            existingDoc.DowngradeProhibitFlag = !string.IsNullOrWhiteSpace(patchEvent.FlyingClubMemberDocument.DowngradeProhibitFlag) ? patchEvent.FlyingClubMemberDocument.DowngradeProhibitFlag : existingDoc.DowngradeProhibitFlag;
            dto = new(existingDoc);

            return new CosmosDocumentEvent(dto, transactionEvent.EventType);
        }

        private async Task PublishEvent(EAPI.Domain.Events.CloudEvent cloudEvent)
        {
            await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });

            try
            {
                await _mediator.Publish(cloudEvent);
            }
            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                throw;
            }
        }
    }
}
