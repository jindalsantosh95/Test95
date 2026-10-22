using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Models
{
    public class Campaigns
    {
        public string RowId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string RegistrationRequired { get; set; }
        public string WebRegistrationRequired { get; set; }
        public DateTime? CampaignStartDate { get; set; }
        public DateTime? CampaignEndDate { get; set; }
        public string Purpose { get; set; }
        public string CampaignCode { get; set; }
        public string StatusCode { get; set; }
        public string TemplateUrl { get; set; }
        public string CampaignPurpose { get; set; }
        public string SaveEmailsSent { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string DblastpdatedSrc { get; set; }
        public DateTime? DblastUpdatedOn { get; set; }
    }
}
