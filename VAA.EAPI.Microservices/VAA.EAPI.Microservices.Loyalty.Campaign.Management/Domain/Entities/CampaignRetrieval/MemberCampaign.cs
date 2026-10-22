using System;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval
{
    public class MemberCampaign
    {
        public string Id { get; set; }
        public DateTime? AssignmentDate { get; set; }
        public DateTime? ContactedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string DocumentStatusCode { get; set; }
        public string Comments { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string CampaignId { get; set; }
        public string CustomerId { get; set; }
        public string CampaignName { get; set; }
        public string CampaignCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
