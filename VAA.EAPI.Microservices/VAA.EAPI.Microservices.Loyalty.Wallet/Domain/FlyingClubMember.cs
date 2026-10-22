using System;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain
{
    public class FlyingClubMember
    {
        public DateTime CreatedOn { get; set; }
        public string MemberNumber { get; set; }
        public int TotalTierPoints { get; set; }
        public int VsOnlyTotalTierPoints { get; set; }
        public string TierCode { get; set; }
        public string MemberType { get; set; }
        public string HeadOfGroupMemberNumber { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public MemberShipCardDetails memberShipCardDetails { get; set; }
    }
}
