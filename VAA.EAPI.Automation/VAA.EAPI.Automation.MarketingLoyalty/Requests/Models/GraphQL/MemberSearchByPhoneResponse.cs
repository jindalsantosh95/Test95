namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL
{
    public class MemberSearchByPhoneResponse
    {
        public string envelopeSalutation { get; set; }
        public string memberNumber { get; set; }
        public DateTime birthDate { get; set; }
        public string currentTier { get; set; }
        public string emailAddress { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string addressLine3 { get; set; }
        public string city { get; set; }
        public string region { get; set; }
        public string country { get; set; }
        public string postCode { get; set; }
        public string phoneType { get; set; }
        public string phoneNumber { get; set; }
        public string phoneCountryCode { get; set; }
        public DateTime enrollmentDate { get; set; }
        public string memberPointBalance { get; set; }
    }

}
