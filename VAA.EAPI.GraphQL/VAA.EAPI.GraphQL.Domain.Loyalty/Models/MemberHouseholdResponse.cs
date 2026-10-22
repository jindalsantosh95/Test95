using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class MemberHouseholdResponse
    {
        public IEnumerable<HouseholdMember> HouseholdMembers { get; set; }
        public HeadOfHousehold HeadOfHousehold { get; set; }
        public IEnumerable<HouseholdHistoryItem> HouseholdHistory { get; set; }

        public MemberHouseholdResponse() { }
    }

}
