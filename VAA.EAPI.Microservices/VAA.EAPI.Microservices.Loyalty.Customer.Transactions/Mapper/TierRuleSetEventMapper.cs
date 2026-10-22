using System;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper
{
    public class TierRuleSetEventMapper
    {
        public static TierRuleSetEntity MapTierRuleSetEventData(FlyingClubMember existingDoc, FlyingClubMemberCommandBase patchEvent)
        {
            TierRuleSetEntity tierRuleSetEntity = new();
            //Default Value FirstVSFlightActivity
            tierRuleSetEntity.FirstVSFlightActivity = Constants.Application.FirstVSFlightActivityDefaultValue;
            foreach (MemberAttribute memberAttribute in existingDoc.MemberAttributes)
            {
                MemberAttribute memberAttribute1 = existingDoc.MemberAttributes?.Where(x => x.AttributeName.ToUpper() == memberAttribute.AttributeName.ToUpper()).FirstOrDefault();
                switch (memberAttribute1?.AttributeName.ToUpper())
                {
                    case Constants.Application.S2GFlag:
                        tierRuleSetEntity.S2GFlag = memberAttribute1?.Value;
                        break;
                    case Constants.Application.S2SFTGFlag:
                        tierRuleSetEntity.S2SFTGFlag = memberAttribute1?.Value;
                        break;
                    case Constants.Application.S2SFlag:
                        tierRuleSetEntity.S2SFlag = memberAttribute1?.Value;
                        break;
                    case Constants.Application.FirstVSFlightActivity:
                        tierRuleSetEntity.FirstVSFlightActivity = memberAttribute1?.Value;
                        break;
                }
            }
            tierRuleSetEntity.FastTrackId = existingDoc.FastTrackId;
            tierRuleSetEntity.VAInstantGoldId = existingDoc.VAInstantGoldCampaignId;
            tierRuleSetEntity.InstantSilverId = existingDoc.InstantSilverCampaignId;
            tierRuleSetEntity.CustomerId = existingDoc.CustomerId;
            tierRuleSetEntity.CurrentStatus = existingDoc.Status;
            tierRuleSetEntity.CurrentTier = existingDoc.TierCode;
            tierRuleSetEntity.MemberType = existingDoc.MemberType;
            tierRuleSetEntity.TierName = existingDoc.TierCode;
            tierRuleSetEntity.MemberNumber = existingDoc.MemberNumber;
            tierRuleSetEntity.ReferSSAGoldId = patchEvent.FlyingClubMemberDocument.SSAMemberNumber;
            tierRuleSetEntity.SsaGoldTierName = patchEvent.HeadTierCode;
            tierRuleSetEntity.CenturionFlag = existingDoc.CenturianFlag;
            tierRuleSetEntity.DowngradeProhibitFlag = existingDoc.DowngradeProhibitFlag;

            return tierRuleSetEntity;
        }
        public static CloudEvent MapTierRuleSetEvent(TierRuleSetEntity tierRuleSetEntity)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Data = tierRuleSetEntity,
                Type = Constants.Application.TierRuleSetEvent,
                Source = ConfigReader.Get(Constants.EnvironmentSettings.RuleEngineTopicName),
                Subject = Constants.Application.TierRuleSetEvent,
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion
            };
        }
    }
}
