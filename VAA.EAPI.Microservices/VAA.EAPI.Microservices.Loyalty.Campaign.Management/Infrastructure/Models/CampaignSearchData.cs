using System;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models
{
    public class CampaignSearchData
    {
        public string CampaignId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string RegistrationRequired { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CampaignCode { get; set; }
        public string Status { get; set; }
        public string LinkToTemplate { get; set; }
        public string CampaignPurpose { get; set; }
        public string Type { get; set; }
        public string Promotion_Name { get; set; }
        public string Promotion_Code { get; set; }
        public DateTime? Promotion_StartDate { get; set; }
        public DateTime? Promotion_EndDate { get; set; }
        public string Partner_Name { get; set; }
        public string Promotion_LastUpdBy { get; set; }
    }
}
