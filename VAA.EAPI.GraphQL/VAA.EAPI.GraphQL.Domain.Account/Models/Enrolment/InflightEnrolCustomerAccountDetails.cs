using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
{
    public class InflightEnrolCustomerAccountDetails
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public AccountContactDetails? ContactDetails { get; set; }
        public CustomerAccountTypeEnum? CustomerType { get; set; }
        public string EnrollmentChannel { get; set; }
        public IEnumerable<AccountPreference>? Preferences { get; set; }
        public required AccountLoyaltyMember LoyaltyMember { get; set; }
    }
}
