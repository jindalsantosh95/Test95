namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionUpdate
{
    public class PromotionUpdateRequestData
    {
        public string PromotionId { get; set; }
        public string PromotionName { get; set; }
        public string PartnerName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Active { get; set; }
        public string CampaignLinked { get; set; }
        public string InitiatedBy { get; set; }

    }
}
