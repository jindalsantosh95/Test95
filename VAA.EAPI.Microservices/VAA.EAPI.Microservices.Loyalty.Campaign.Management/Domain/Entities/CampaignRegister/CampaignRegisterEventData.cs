using System;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister
{
    public class CampaignRegisterEventData
    {
        public string CustomerId { get; set; }
        public string CampaignCode { get; set; }
        public string CampaignId { get; set; }
        public string CreatedBy { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CampaignType { get; set; }
        public string DocumentId { get; set; }
        public string DocumentStatus { get; set; }
        public string MemberNumber { get; set; }
        public DateTime? CampaignStartDate { get; set; }
        public DateTime? CampaignEndDate { get; set; }

    }
}