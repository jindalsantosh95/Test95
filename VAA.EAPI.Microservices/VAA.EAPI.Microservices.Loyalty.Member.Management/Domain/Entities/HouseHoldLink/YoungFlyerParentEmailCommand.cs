namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.HouseHoldLink
{
    public class YoungFlyerParentEmailCommand
    {
        public string Email { get; set; }
        public string EmailTemplate { get; set; }
        public YoungFlyerParentEmailData AdobeInviteMemberRequest { get; set; }
    }

    public class YoungFlyerParentEmailData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MembershipNumber { get; set; }
        public string FirstNameOfHouseholdLead { get; set; }
        public string FirstNameOfHouseholdMember { get; set; }
    }
}
