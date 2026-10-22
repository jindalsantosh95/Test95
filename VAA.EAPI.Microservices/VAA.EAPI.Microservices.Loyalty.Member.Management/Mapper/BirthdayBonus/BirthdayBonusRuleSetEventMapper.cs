using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BirthdayBonus
{
    public class BirthdayBonusRuleSetEventMapper
    {
        public static BirthdayBonusRuleSetEntity MapBirthdayBonusRuleSetEventData(BirthdayBonusMemberDetails details, Product productDetails, string memberStatus)
        {
            return new BirthdayBonusRuleSetEntity()
            {
                CustomerId = details.CustomerId,
                MemberNumber = details.MemberNumber,
                ProductName = productDetails.ProductName,
                ProductId = productDetails.ProductId,
                MemberStatus = memberStatus
            };
        }
        public static CloudEvent MapBirthdayBonusRuleSetEvent(BirthdayBonusRuleSetEntity birthdayBonusRuleSetEntity)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Data = birthdayBonusRuleSetEntity,
                Type = Constants.Application.BirthdayBonusCombinedRuleSetEvent,
                Source = ConfigReader.Get(Constants.Environment.RuleEngineTopicName),
                Subject = Constants.Application.BirthdayBonusCombinedRuleSetEvent,
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion
            };
        }
    }
}
