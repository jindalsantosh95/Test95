namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment
{
    public class LoyaltyMember
    {
        public enum ProgramNameEnum
        {
            VirginAtlanticFlyingClub
        }
        public ProgramNameEnum? ProgramName { get; set; }

        public enum LoyaltyMemberStatusEnum
        {
            Active,
            Expired,
            Frozen,
            Inactive,
            Merged,
            Deceased,
            Suspended,
            Cancelled,
            ActivePending,
            Fraud,
            PendingCancellation,
            RouteDeal
        }

        public LoyaltyMemberStatusEnum? Status { get; set; }
        public enum MemberTypes
        {
            Individual,
            Household,
            Account
        }
        public MemberTypes? MemberType { get; set; }
        public string? CampaignCode { get; set; }
        public bool? TestMember { get; set; }
        public string? MediaCode { get; set; }
        public string? EnrollmentChannel { get; set; }
        public bool? TermsAndConditionsAccepted { get; set; }
        public GuardianDetails? GuardianDetails { get; set; }
        public PartnerMember? PartnerMember { get; set; }
        public string? MemberNumber { get; set; }
        public string? FlyingClubCardNumber { get; set; }

    }
}
