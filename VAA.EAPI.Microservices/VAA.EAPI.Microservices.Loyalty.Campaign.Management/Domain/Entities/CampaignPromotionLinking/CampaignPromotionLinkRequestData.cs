namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignPromotionLinking
{
    public class CampaignPromotionLinkRequestData
    {
        public string CampaignId { get; set; }
        public string PromotionId { get; set; }
        public string CreatedBy { get; set; }
    }
}
