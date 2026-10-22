using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
{
    public class CustomerAccountDetails
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? PreferredFirstName { get; set; }
        public string? PreferredLastName { get; set; }
        public string? PreferredGender { get; set; }
        public string? PreferredName { get; set; }
        public string? PreferredSalutation { get; set; }
        public string? PreferredPronouns { get; set; }
        public string Gender { get; set; }
        public string? Title { get; set; }
        public string? Salutation { get; set; }
        public string? Nationality { get; set; }

        public CustomerAccountTypeEnum? CustomerType { get; set; }
        public string DateOfBirth { get; set; }
        public string? Password { get; set; }

        public List<AccountAddressDetail>? AddressDetails { get; set; }
        public AccountContactDetails? ContactDetails { get; set; }
        public AccountLoyaltyMember LoyaltyMember { get; set; }
        public AccountTravelProfile? TravelProfile { get; set; }
        public IEnumerable<AccountPreference>? Preferences { get; set; }
        public IEnumerable<AccountCustomerAttribute>? CustomerAttribute { get; set; }
        public string? FcNumber { get; set; }
        public string? CustomerId { get; set; }
        public string? Status { get; set; }
        public string? EnrolmentChannel { get; set; }
        public string? ContinuationToken { get; set; }
    }
}
