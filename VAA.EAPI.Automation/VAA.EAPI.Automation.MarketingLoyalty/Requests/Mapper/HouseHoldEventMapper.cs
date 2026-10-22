using VAA.EAPI.Automation.MarketingLoyalty.Requests.Events;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Mapper
{
    public class HouseHoldEventMapper
    {
        public static CloudEvent MapHouseholdEvent(HouseholdDissolveMember householdDissolveMemberDetails, string serviceBusTopicName)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Data = householdDissolveMemberDetails,
                Type = Constants.Application.DissolveHouseholdAccounts,
                Source = serviceBusTopicName,
                Subject = Constants.Application.DissolveHouseholdAccounts,
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion,
                DataContentType = Constants.Application.DataContentType
            };
        }
        public static HouseholdDissolveMember HouseholdMemberDetails(string headOfGroupCustomerId, string groupId, string initiatedBy)
        {
            return new HouseholdDissolveMember
            {
                CustomerId = headOfGroupCustomerId,
                InitiatedBy = initiatedBy,
                MemberId = null,
                GroupId = groupId
            };
        }
    }
}
