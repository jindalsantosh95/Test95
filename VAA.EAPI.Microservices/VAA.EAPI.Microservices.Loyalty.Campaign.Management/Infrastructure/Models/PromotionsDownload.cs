using System;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models
{
    public class PromotionsDownload
    {
        public string? PromotionId { get; set; }
        public string? PromotionName { get; set; }
        public string? PartnerName { get; set; }
        public string? Active { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CampaignLinked { get; set; }
        public string? PartnerId { get; set; }
    }
}
