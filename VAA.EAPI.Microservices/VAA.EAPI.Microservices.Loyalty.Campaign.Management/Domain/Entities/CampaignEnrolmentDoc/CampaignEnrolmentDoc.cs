namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignEnrolmentDoc
{
    public class CampaignEnrolmentDocument
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string CreatedBy { get; set; }
        public string PromotionId { get; set; }
        public string DocumentStatus { get; set; }
    }
}
