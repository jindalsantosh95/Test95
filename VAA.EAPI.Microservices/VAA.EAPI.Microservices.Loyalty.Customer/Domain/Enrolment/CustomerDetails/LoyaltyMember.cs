namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails
{
    public class LoyaltyMember
    {
        public MemberTypes? MemberType { get; set; }
        public ProgramNameEnum? ProgramName { get; set; }
        public string MemberNumber { get; set; }
        public string FlyingClubCardNumber { get; set; }
        public StatusEnum? Status { get; set; }
        public string CampaignCode { get; set; }
        public bool TestMember { get; set; }
        public string MediaCode { get; set; }
        public string EnrollmentChannel { get; set; }
        public bool TermsAndConditionsAccepted { get; set; }
        public Guardian GuardianDetails { get; set; }
        public PartnerMember PartnerMember { get; set; }

    }
}
