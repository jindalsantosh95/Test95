using VAA.EAPI.Automation.MarketingLoyalty.Requests.Events;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Mapper
{
    public class MillionMillerAwardEventMapper
    {
        public static CloudEvent MillionMillerAwardEvent(MillionMillerAwardMemberDetails millionMillerAwardMemberDetails, string serviceBusTopicName)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Data = millionMillerAwardMemberDetails,
                Type = Constants.Application.MillionMilerAwardEvent,
                Source = serviceBusTopicName,
                Subject = Constants.Application.MillionMilerAwardEvent,
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion,
                DataContentType = Constants.Application.DataContentType
            };
        }
        public static MillionMillerAwardMemberDetails MillionMillerAwardMemberDetails(string customerId, string memberNumber, string initiatedBy)
        {
            return new MillionMillerAwardMemberDetails
            {
                CustomerId = customerId,
                InitiatedBy = initiatedBy,
                MemberId = memberNumber,
                FirstName = null,
                LastName = null,
                Email = null
            };
        }
    }
}
