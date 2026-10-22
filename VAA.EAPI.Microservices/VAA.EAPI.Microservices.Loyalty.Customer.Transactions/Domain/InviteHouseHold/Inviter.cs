using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold
{
    public class Inviter : Audit
    {
        public string Id { get; set; }
        public string GroupName { get; set; }
        public string GroupTypeCode { get; set; }
        public string Status { get; set; }
        public string InviterMemberNumber { get; set; }
        public string InviteeMemberNumber { get; set; }

        public static explicit operator Inviter(InviteHouseHoldCreateRequest inviteHouseHold) => new()
        {
            CreatedBy = inviteHouseHold.InviterMemberId,
            CreatedOn = DateTime.UtcNow,
            LastUpdatedBy = inviteHouseHold.InviterMemberId,
            LastUpdatedOn = DateTime.UtcNow,
            InviterMemberNumber = inviteHouseHold.InviterMemberId,
            Id = Guid.NewGuid().ToString(),
            GroupName = inviteHouseHold.GroupName,
            GroupTypeCode = Constants.Application.HouseholdGroupTypeCode,
            Status = Constants.Application.ActiveStatus,
            InviteeMemberNumber = inviteHouseHold.InviteeMemberId
        };
    }
}
