using System;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch
{
    public class PromotionData
    {
        public string PromotionId { get; set; }
        public string PromotionName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PartnerName { get; set; }
        public string LastModifiedBy { get; set; }
    }
}
