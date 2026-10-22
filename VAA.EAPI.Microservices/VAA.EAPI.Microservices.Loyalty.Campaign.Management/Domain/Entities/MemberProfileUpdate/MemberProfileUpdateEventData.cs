namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.MemberProfileUpdate
{
    public class MemberProfileUpdateEventData
    {
        public string CustomerId { get; set; }
        public FlyingClubMemberDocument FlyingClubMemberDocument { get; set; }

    }
}