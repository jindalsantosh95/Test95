using Newtonsoft.Json.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Domain.Virgin.Models.Payment
{
    public class PaymentPayer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Contacts PayerContacts { get; set; }
        public Address PayerAddress { get; set; }
        public LoyaltyMemberDetails LoyaltyMemberDetails { get; set; }
        public bool LoyaltyMemberTravelling { get; set; }
        public string PayerType { get; set; }

        public static explicit operator PaymentPayer(JObject source)
        {
            return new PaymentPayer()
            {
                FirstName = source["FirstName"]?.ToString(),
                LastName = source["LastName"]?.ToString(),
                PayerContacts = source["PayerContacts"] != null ? (Contacts)source["PayerContacts"] : null,
                PayerAddress = source["PayerAddress"] != null ? (Address)source["PayerAddress"] : null,
                LoyaltyMemberDetails = source["LoyaltyMemberDetails"] != null ? (LoyaltyMemberDetails)source["LoyaltyMemberDetails"] : null,
                LoyaltyMemberTravelling = source["LoyaltyMemberTravelling"] != null ? (bool)source["LoyaltyMemberTravelling"] : false,
                PayerType = source["PayerType"]?.ToString()
            };
        }
    }
}
