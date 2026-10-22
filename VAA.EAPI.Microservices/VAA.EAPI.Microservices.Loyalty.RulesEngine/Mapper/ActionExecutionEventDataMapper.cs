using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Transaction;
using Action = VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine.Action;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Mapper
{
    public class ActionExecutionEventDataMapper
    {
        public static FlyingClubMemberUpdateEventData MapSetTierFCMDocEventData(Action action, string customerId)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            MemberShipCardDetails membershipCardDetails = new MemberShipCardDetails
            {
                LastUpdatedBy = Constants.Application.SystemName,
                Active = Constants.Application.ActiveFlagValue,
                CardStatusCode = Constants.Application.CardStatus,
                TierCode = action.Value,
                CardType = action.Value,
                IssueDate = DateTime.UtcNow,
                StartDate = DateTime.Today.AddDays(1),
                ExpiryDate = DateTime.UtcNow.AddMonths(13).AddDays(-Convert.ToInt32(DateTime.UtcNow.Day)).Date,
                RequestedDate = DateTime.UtcNow,
                CardExpiryDate = DateTime.UtcNow.AddMonths(13).AddDays(-Convert.ToInt32(DateTime.UtcNow.Day)).Date,

            };
            FlyingClubMemberDocument flyingClubMemberDocument = new FlyingClubMemberDocument
            {
                LastUpdatedBy = Constants.Application.SystemName,
                CustomerId = customerId,
                TierCode = action.Value,
                GoldStartDate = action.Value == Constants.Application.GoldTier ? DateTime.Today.AddDays(1) : null,
                MemberShipCardDetails = membershipCardDetails
            };
            return new FlyingClubMemberUpdateEventData
            {
                CustomerId = customerId,
                FlyingClubMemberDocument = flyingClubMemberDocument
            };
        }

        public static MembershipChangeDocumentEventData MapMembershipChangeCreateEventData(Action action, RuleActionsResponse actionExecutionRequest)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            MembershipChangeDocument membershipChangeDocument = new MembershipChangeDocument
            {
                CreatedBy = Constants.Application.SystemName,
                LastUpdatedBy = Constants.Application.SystemName,
                TierApprovalStatus = Constants.Application.TierApprovalStatus,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.UtcNow.AddMonths(13).AddDays(-Convert.ToInt32(DateTime.UtcNow.Day)).Date,
                TierCode = action.Value,
                CustomerId = actionExecutionRequest.CustomerId,
                Active = Constants.Application.ActiveFlagValue,
                ApprovalDate = DateTime.UtcNow,
                MemberNumber = actionExecutionRequest.MemberNumber
            };
            return new MembershipChangeDocumentEventData
            {
                MembershipChangeDocument = membershipChangeDocument
            };
        }

        public static MembershipChangeDocumentEventData MapMembershipChangeUpdateEventData(Action action, string customerId)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            MembershipChangeDocument membershipChangeDocument = new MembershipChangeDocument
            {
                LastUpdatedBy = Constants.Application.SystemName,
                Active = Constants.Application.InActiveFlagValue,
                CustomerId = customerId,
                EndDate = DateTime.UtcNow
            };
            return new MembershipChangeDocumentEventData
            {
                MembershipChangeDocument = membershipChangeDocument

            };
        }

        public static FlyingClubMemberUpdateEventData MapFlagsFCMDocEventData(Action action, string customerId)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            MemberAttributeDocument memberAttribute = new MemberAttributeDocument();
            switch (action.Name)
            {
                case Constants.Application.StraightToGoldFlagAction:
                    memberAttribute.AttributeName = Constants.Application.StraightToGoldFlag;
                    memberAttribute.Value = action.Value;
                    break;
                case Constants.Application.StraightToSilverFlagAction:
                    memberAttribute.AttributeName = Constants.Application.StraightToSilverFlag;
                    memberAttribute.Value = action.Value;
                    break;
                case Constants.Application.StraightToSilverFTGFlagAction:
                    memberAttribute.AttributeName = Constants.Application.StraightToSilverFTGFlag;
                    memberAttribute.Value = action.Value;
                    break;
                case Constants.Application.SetEarnActivityFlagAction:
                    memberAttribute.AttributeName = Constants.Application.EarnActivityFlag;
                    memberAttribute.Value = action.Value;
                    break;
            }
            FlyingClubMemberDocument flyingClubMemberDocument = new FlyingClubMemberDocument
            {
                LastUpdatedBy = Constants.Application.SystemName,
                CustomerId = customerId,
                MemberAttributes = new List<MemberAttributeDocument>() { memberAttribute }
            };
            return new FlyingClubMemberUpdateEventData
            {
                CustomerId = customerId,
                FlyingClubMemberDocument = flyingClubMemberDocument
            };
        }

        public static FlyingClubMemberUpdateEventData MapDowngradeProhibitFlagFCMDocEventData(Action action, string customerId)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            FlyingClubMemberDocument flyingClubMemberDocument = new FlyingClubMemberDocument
            {
                LastUpdatedBy = Constants.Application.SystemName,
                CustomerId = customerId,
                DowngradeProhibitFlag = action.Value
            };

            return new FlyingClubMemberUpdateEventData
            {
                CustomerId = customerId,
                FlyingClubMemberDocument = flyingClubMemberDocument
            };
        }

        public static TransactionCreateEventData MapBirthdayBonusTransactionCreateEventData(Action action, RuleActionsResponse actionsResponse, string partnerId, string statementText)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            TransactionDocument transactionDocument = new TransactionDocument
            {
                CustomerId = actionsResponse.CustomerId,
                CreatedBy = Constants.Application.BirthdayBonusUser,
                LastUpdatedBy = Constants.Application.BirthdayBonusUser,
                MemberNumber = actionsResponse.MemberNumber,
                TransactionDate = DateTime.UtcNow.Date.ToString(Constants.Application.TxnDateTimeFormat),
                ProductId = actionsResponse.ProductId,
                PartnerId = partnerId,
                TransactionType = Constants.Application.AccrualTrxnType,
                TransactionSubType = Constants.Application.AccrualTrxnSubType,
                TransactionValue = action.Value,
                TransactionChannel = Constants.Application.BirthdayBonusTrxnChannel,
                ActivityDate = DateTime.UtcNow.Date.ToString(Constants.Application.TxnDateTimeFormat),
                StatusCode = Constants.Application.SuccessTrxnStatus,
                StatementText = statementText,
                CreditDetails = new List<Creditdetail>
                {
                    new Creditdetail
                    {
                        CreatedBy = Constants.Application.BirthdayBonusUser,
                        LastUpdatedBy = Constants.Application.BirthdayBonusUser,
                        CreditValue= action.Value,
                        PromotionRuleActionId= action.ActionID,
                        PromotionId=action.PromotionID,
                        PointType= Constants.Application.MilesPointType,
                        PointSubType=Constants.Application.BaseMilesPointSubType,
                        QualifyingFlag = Constants.Application.MilesQualifyingFlag
                    }
                }
            };
            return new TransactionCreateEventData
            {
                TransactionDocument = transactionDocument
            };
        }
    }
}
