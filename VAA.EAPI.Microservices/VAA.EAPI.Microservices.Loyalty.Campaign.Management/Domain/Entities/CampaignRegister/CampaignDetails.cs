using System;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister
{
    public class CampaignDetails
    {
        public string CustomerId { get; set; }
        public string CampaignCode { get; set; }
        public DateTime? CampaignStartDate { get; set; }
        public DateTime? CampaignEndDate { get; set; }
        public string CampaignId { get; set; }
        public string CampaignType { get; set; }
        public string RegistrationRequired { get; set; }
        public string CampaignStatus { get; set; }
        public string CampaignName { get; set; }
        public string CampaignDescription { get; set; }

    }
}
