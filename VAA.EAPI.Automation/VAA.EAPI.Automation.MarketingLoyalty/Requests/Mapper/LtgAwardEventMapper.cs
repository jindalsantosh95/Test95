
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Events;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Mapper
{
    public class LtgAwardEventMapper
    {

        public static CloudEvent LtgAwardEvent(LtgAwardMemberDetails ltgAwardMemberDetails, string serviceBusTopicName)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Data = ltgAwardMemberDetails,
                Type = Constants.Application.LTGAwardEvent,
                Source = serviceBusTopicName,
                Subject = Constants.Application.LTGAwardEvent,
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion,
                DataContentType = Constants.Application.DataContentType
            };
        }
        public static LtgAwardMemberDetails LtgAwardMemberDetails(string customerId, string memberNumber, string initiatedBy)
        {
            return new LtgAwardMemberDetails
            {
                CustomerId = customerId,
                InitiatedBy = initiatedBy,
                MemberId = memberNumber
            };
        }
    }
}
