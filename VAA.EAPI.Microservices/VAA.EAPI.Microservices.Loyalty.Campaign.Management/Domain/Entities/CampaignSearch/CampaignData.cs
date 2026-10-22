using System;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch
{
    public class CampaignData
    {
        public string CampaignId { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CampaignCode { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string CampaignPurpose { get; set; }
        public string RegistrationRequired { get; set; }
        public string LinkToTemplate { get; set; }
        public PromotionData Promotion { get; set; }
    }
}
