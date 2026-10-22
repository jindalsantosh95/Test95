using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold
{
    public class InviteHouseHold : InviteHouseHoldCreateRequest
    {
        public InviteHouseHold(InviteHouseHoldDTO inviteHouseHoldDTO)
        {
            GroupName = inviteHouseHoldDTO.GroupName;
            InviterMemberId = inviteHouseHoldDTO.InviterMemberNumber;
            InviteeMemberId = inviteHouseHoldDTO.InviteeMemberNumber;
        }
    }
}
