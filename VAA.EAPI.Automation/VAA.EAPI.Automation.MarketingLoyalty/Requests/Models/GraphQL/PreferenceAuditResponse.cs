namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL
{
    public class PreferenceAuditResponse
    {
        public string customerId { get; set; }
        public string language { get; set; }
        public string doNotSMS { get; set; }
        public string doNotCall { get; set; }
        public string doNotEmail { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string preferredPartnerFFP { get; set; }
        public string preferenceUpdatedBy { get; set; }
        public DateTime? preferenceUpdatedOn { get; set; }
        public string sourceIdentifier { get; set; }
        public string vAMarketingPreference { get; set; }
    }
}
