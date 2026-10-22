using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS
{
    public class EnrolCustomerRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PreferredFirstName { get; set; }
        public string PreferredLastName { get; set; }
        public string PreferredGender { get; set; }
        public string PreferredName { get; set; }
        public string PreferredSalutation { get; set; }
        public string PreferredPronouns { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public string Salutation { get; set; }
        public string Nationality { get; set; }
        public CustomerAccountTypeEnum CustomerType { get; set; }
        public string DateOfBirth { get; set; }
        public string Password { get; set; }
        public IEnumerable<UpgradeAddressRequest> AddressDetails { get; set; }
        public Contact ContactDetails { get; set; }
        public string MemberType { get; set; }
        public string EnrollmentChannel { get; set; }
        public string MediaCode { get; set; }
        public bool TestMember { get; set; }
        public string Status { get; set; }
        public bool TermsAndConditionsAccepted { get; set; }
        public IEnumerable<AccountPreference> Preferences { get; set; }
        public string FcNumber { get; set; }
        public string CustomerId { get; set; }
    }
}
