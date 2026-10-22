using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails
{
    public class Customer
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

        [JsonConverter(typeof(StringEnumConverter))]
        public enum CustomerTypeEnum
        {
            [EnumMember(Value = "Basic")]
            Basic = 0,
            [EnumMember(Value = "Loyalty")]
            Loyalty = 1
        }

        public CustomerTypeEnum? CustomerType { get; set; }
        public string DateOfBirth { get; set; }
        public string Password { get; set; }
        public List<Address> AddressDetails { get; set; }
        public Contact ContactDetails { get; set; }
        public LoyaltyMember LoyaltyMember { get; set; }
        public TravelProfile TravelProfile { get; set; }
        public List<Preferences> Preferences { get; set; }
        public List<CustomerAttribute> CustomerAttribute { get; set; }
    }
}
