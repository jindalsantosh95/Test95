using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile
{
    public class CustomerInformation
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CustomerTypeEnum
        {
            Basic = 1,
            Loyalty = 2
        }

        public CustomerTypeEnum? CustomerType { get; set; }

        public string PreferredFirstName { get; set; }
        public string PreferredLastName { get; set; }
        public string PreferredGender { get; set; }
        public string PreferredSalutation { get; set; }
        public string PreferredPronouns { get; set; }
        public string Nationality { get; set; }
        public string LoyaltyNumber { get; set; }
        public string Status { get; set; }
        public string HeadOfGroupMemberNumber { get; set; }
        public string MemberAccountType { get; set; }
        public string MemberTier { get; set; }
        public string SuppSilverHeadAccount { get; set; }
        public string TermsAndConditionsAccepted { get; set; }
        public string DateOfBirth { get; set; }
        public List<Address> AddressDetails { get; set; }
        public Contact ContactDetails { get; set; }
        public List<Award> AwardDetails { get; set; }
        public string PNRName { get; set; }
    }
}
