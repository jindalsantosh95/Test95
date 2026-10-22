namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public sealed class FlyingClubAccount
    {
        public string MemberNumber { get; set; }
        public string TierCode { get; set; }
        public int TotalTierPoints { get; set; }
        public int VirginTierPoints { get; set; }
        public string MemberType { get; set; }
        public string HeadOfGroupMemberNumber { get; set; }
    }
}
