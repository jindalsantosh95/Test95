namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI
{
    public class Name
    {
        public string title { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string formalSalutation { get; set; }
        public string informalSalutation { get; set; }
        public string envelopeSalutation { get; set; }
    }

    public class SearchMembers
    {
        public string accountNumber { get; set; }
        public string accountStatus { get; set; }
        public string tier { get; set; }
        public string tierPoints { get; set; }
        public int? milesBalance { get; set; }
        public string milesExpiryDate { get; set; }
        public string emailAddress { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public Name name { get; set; }
        public string address { get; set; }
        public List<string> phoneNumbers { get; set; }
        public string pointDotComEligible { get; set; }
        public string customerId { get; set; }
    }
}
