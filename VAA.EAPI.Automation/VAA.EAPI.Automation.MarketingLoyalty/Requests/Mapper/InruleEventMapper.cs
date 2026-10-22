using VAA.EAPI.Automation.MarketingLoyalty.Requests.Events;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Mapper
{
    public class InruleEventMapper
    {
        public static CloudEvent MapBirthdayBonusRuleSetEvent(BirthdayBonusMemberDetails memberBirthdayDetails, string serviceBusTopicName)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Data = memberBirthdayDetails,
                Type = Constants.Application.BirthdayBonusCombinedRuleSetEvent,
                Source = serviceBusTopicName,
                Subject = Constants.Application.BirthdayBonusCombinedRuleSetEvent,
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion,
                DataContentType = Constants.Application.DataContentType
            };
        }

        public static BirthdayBonusMemberDetails MemberDetails(string customerId, string productName, string memberNumber, string productId, string memberStatus)
        {
            return new BirthdayBonusMemberDetails
            {
                CustomerId = customerId,
                MemberNumber = memberNumber,
                ProductName = productName,
                ProductId = productId,
                MemberStatus = memberStatus
            };
        }
    }
}

