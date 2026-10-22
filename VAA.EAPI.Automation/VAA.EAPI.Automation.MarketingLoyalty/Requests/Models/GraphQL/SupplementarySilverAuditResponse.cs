namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL
{
    public class SupplementarySilverAuditResponse
    {
        public string auditCategory { get; set; }
        public string auditSubCategory { get; set; }
        public string sourceIdentifier { get; set; }
        public string relationshipType { get; set; }
        public string auditData { get; set; }
        public string dataUpdatedBy { get; set; }
        public DateTime? dataUpdatedOn { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string customerId { get; set; }
    }

}
