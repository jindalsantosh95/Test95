namespace VAA.EAPI.Microservices.Loyalty.Vred.Domain
{
    public class EventData
    {
        public string MemberNumber { get; set; }
        public string Status { get; set; }
        public string Source { get; set; }
        public string CurrentTierStartDate { get; set; }
        public string CurrentTierEndDate { get; set; }
        public string CustomerId { get; set; }
        public string AccountType { get; set; }
        public string EnrolmentDateTime { get; set; }
        public string EnrolmentChannel { get; set; }
        public string CampaignPromotionIdentifier { get; set; }
        public string OCDBSyncFlag { get; set; }
        public string MediaCode { get; set; }
        public string CampaignCode { get; set; }
        public string DefaultTierValue { get; set; }
        public string TestMember { get; set; }
    }
}
