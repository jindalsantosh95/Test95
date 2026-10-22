using System;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models
{
    public class CampaignsPromotionsMapping
    {
        public string RowId { get; set; }
        public string CampaignId { get; set; }
        public string PromotionId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }

    }
}