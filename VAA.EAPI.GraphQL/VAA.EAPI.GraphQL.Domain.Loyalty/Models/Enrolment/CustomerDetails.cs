using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment
{
    public class CustomerDetails
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
        public string? Gender { get; set; }
        public string? Title { get; set; }
        public string? Salutation { get; set; }
        public string? Nationality { get; set; }
        public CustomerTypeEnum? CustomerType { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Password { get; set; }
        public List<AddressDetail>? AddressDetails { get; set; }
        public ContactDetails? ContactDetails { get; set; }
        public LoyaltyMember? LoyaltyMember { get; set; }
        public TravelProfile? TravelProfile { get; set; }
        public List<Preference>? Preferences { get; set; }
        public List<CustomerAttribute>? CustomerAttribute { get; set; }

    }
}
