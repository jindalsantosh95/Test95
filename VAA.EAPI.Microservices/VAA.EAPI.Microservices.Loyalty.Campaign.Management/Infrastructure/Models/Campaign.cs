using System;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models
{
    public class Campaign
    {
        public string RowId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string RegistrationRequired { get; set; }
        public string WebRegistrationRequired { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Purpose { get; set; }
        public string CampaignCode { get; set; }
        public string Status { get; set; }
        public string CampaignPurpose { get; set; }
        public string SaveEmailsSent { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string Type { get; set; }

    }
}