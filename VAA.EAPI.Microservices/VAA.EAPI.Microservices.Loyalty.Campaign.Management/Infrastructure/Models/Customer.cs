using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models
{
    public class Customer
    {
        public string CustomerId { get; set; }
        [Newtonsoft.Json.JsonProperty("preferredFirstName")]
        public string FirstName { get; set; }
        [Newtonsoft.Json.JsonProperty("preferredLastName")]
        public string LastName { get; set; }
        [Newtonsoft.Json.JsonProperty("preferredGender")]
        public string Gender { get; set; }
        [Newtonsoft.Json.JsonProperty("preferredSalutation")]
        public string Salutation { get; set; }
        public string Pronouns { get; set; }
        public string Nationality { get; set; }
        public string LoyaltyNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public IEnumerable<Address> AddressDetails { get; set; }
        public Contact ContactDetails { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string MemberAccountType { get; set; }
        public string HeadOfGroupMemberNumber { get; set; }
        public string TermsAndConditionsAccepted { get; set; }
    }
}
