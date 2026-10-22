using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{
    public class Applicantinfo
    {
        [JsonProperty("cipher.firstName")]
        public string CipherfirstName { get; set; }
        [JsonProperty("cipher.lastName")]
        public string CipherlastName { get; set; }
        [JsonProperty("cipher.middleInitial")]
        public string CiphermiddleInitial { get; set; }
        [JsonProperty("cipher.emailAddress")]
        public string CipheremailAddress { get; set; }
        [JsonProperty("cipher.dateOfBirth")]
        public string CipherdateOfBirth { get; set; }        
        public Contactinfo[] ContactInfo { get; set; }
        public Address Address { get; set; }
        public Membershipinfo MembershipInfo { get; set; }
        public int NetIncome { get; set; }
        public string ResidenceType { get; set; }
        public string FinderNumber { get; set; }        
        [JsonProperty("cipher.initialPurchaseAmount")]
        public string CipherinitialPurchaseAmount { get; set; }
        public string PaymentAmount { get; set; }
        public string ChargeLoanDate { get; set; }
        public string CustomerSplitPayAmount { get; set; }        
        [JsonProperty("cipher.socialSecurity")]
        public string CiphersocialSecurity { get; set; }
    }

}
