namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL
{
    public class CustomerProfileAuditResponse
    {
        public string sourceIdentifier { get; set; }
        public string customerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string title { get; set; }
        public DateTime? doB { get; set; }
        public string status { get; set; }
        public string nameChangeReason { get; set; }
        public string authorisedUser { get; set; }
        public string downgradeProhibitFlag { get; set; }
        public string gender { get; set; }
        public DateTime? goldStartDate { get; set; }
        public string telephonePassword { get; set; }
        public string updatedBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string createdBy { get; set; }
        public string customerProfileUpdatedBy { get; set; }
        public DateTime customerProfileUpdatedOn { get; set; }

    }
}
