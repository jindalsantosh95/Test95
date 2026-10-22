namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember
{
    public class FlyingClubMemberPatchEvent
    {
        public FlyingClubMember FlyingClubMemberDocument { get; set; }
        public string CustomerId { get; set; }
        public string HeadTierCode { get; set; }
    }
}
