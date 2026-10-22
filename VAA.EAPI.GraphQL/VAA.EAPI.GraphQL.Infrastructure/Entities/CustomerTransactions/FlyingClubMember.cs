using HotChocolate;
using System;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions
{
    public class FlyingClubMember
    {
        [GraphQLName("Enrolled Date")]
        public DateTime CreatedOn { get; set; }
        [GraphQLName("FCNumber")]
        public string MemberNumber { get; set; }
        [GraphQLName("TierPoints")]
        public int TotalTierPoints { get; set; }
        [GraphQLName("VirginPoints")]
        public int VsOnlyTotalTierPoints { get; set; }
        [GraphQLName("TierName")]
        public string TierCode { get; set; }
        [GraphQLName("MemberType")]
        public string MemberType { get; set; }
        [GraphQLName("FCNumberofHouseHoldHead")]
        public string HeadOfGroupMemberNumber { get; set; }
        [GraphQLName("FcJoinedDate")]
        public DateTime EnrollmentDate { get; set; }
        public MemberShipCardDetails memberShipCardDetails { get; set; }
    }
}
