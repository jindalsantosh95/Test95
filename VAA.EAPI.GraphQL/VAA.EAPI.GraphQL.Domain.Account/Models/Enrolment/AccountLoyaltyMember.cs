namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
{
    public class AccountLoyaltyMember
    {
        public enum AccountProgramNameEnum
        {
            VirginAtlanticFlyingClub
        }
        public AccountProgramNameEnum? ProgramName { get; set; }

        public enum AccountLoyaltyMemberStatusEnum
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

        public AccountLoyaltyMemberStatusEnum? Status { get; set; }
        public enum AccountMemberTypes
        {
            Individual,
            Household,
            Account
        }
        public string MemberType { get; set; }
        public string? CampaignCode { get; set; }
        public bool? TestMember { get; set; }

        public string MediaCode { get; set; }
        public string EnrollmentChannel { get; set; }
        public bool TermsAndConditionsAccepted { get; set; }

        public AccountGuardianDetails? GuardianDetails { get; set; }
        public AccountPartnerMember? PartnerMember { get; set; }
        public string? MemberNumber { get; set; }
        public string? FlyingClubCardNumber { get; set; }

    }
}
