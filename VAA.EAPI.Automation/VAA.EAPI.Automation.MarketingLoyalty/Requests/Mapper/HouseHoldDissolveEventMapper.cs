using VAA.EAPI.Automation.MarketingLoyalty.Requests.Events;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Mapper
{
    public class HouseHoldDissolveEventMapper
    {

        public static CloudEvent MapHouseholdDissolveEvent(HouseholdDissolveMemberOnStatusUpdate HouseholdDissolveMemberDetailsOnStatusUpdate, string serviceBusTopicName)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Data = HouseholdDissolveMemberDetailsOnStatusUpdate,
                Type = Constants.Application.DissolveHouseholdAccountOnStatusUpdate,
                Source = serviceBusTopicName,
                Subject = Constants.Application.DissolveHouseholdAccountOnStatusUpdate,
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion,
                DataContentType = Constants.Application.DataContentType
            };
        }
        public static HouseholdDissolveMemberOnStatusUpdate HouseholdDissolveMemberDetails(string headOfGroupCustomerId, string oldStatus, string newStatus, string memberType, string headOfGroupMemberNumber, string groupId, string initiatedBy)
        {
            return new HouseholdDissolveMemberOnStatusUpdate
            {
                CustomerId = headOfGroupCustomerId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                MemberType = memberType,
                HeadOfGroupMemberNumber = headOfGroupMemberNumber,
                GroupId = groupId,
                InitiatedBy = initiatedBy,

            };
        }

    }
}
