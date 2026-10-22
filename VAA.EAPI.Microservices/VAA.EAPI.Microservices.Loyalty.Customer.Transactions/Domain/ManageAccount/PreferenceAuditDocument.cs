namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount
{
    public class PreferenceAuditDocument
    {
        public string CustomerId { get; set; }
        public string PreferenceUpdatedBy { get; set; }
        public string PreferenceUpdatedOn { get; set; }
        public string DoNotCall { get; set; }
        public string DoNotEmail { get; set; }
        public string DoNotSMS { get; set; }
        public string VAMarketingPreference { get; set; }
        public string PreferredPartnerFFP { get; set; }
    }
}
