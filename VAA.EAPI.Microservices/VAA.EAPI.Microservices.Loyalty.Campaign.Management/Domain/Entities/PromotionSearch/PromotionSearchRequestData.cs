namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionSearch
{
    public class PromotionSearchRequestData
    {
        public string PromotionId { get; set; }
        public string PromotionName { get; set; }
        public string StartDateF { get; set; }
        public string EndDateF { get; set; }
        public string StartDateT { get; set; }
        public string EndDateT { get; set; }
        public string PartnerName { get; set; }
        public string Active { get; set; }

    }
}