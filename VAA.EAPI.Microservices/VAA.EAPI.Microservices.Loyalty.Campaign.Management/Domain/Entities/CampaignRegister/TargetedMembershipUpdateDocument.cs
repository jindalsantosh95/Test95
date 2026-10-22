using System;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister
{
    public class TargetedMembershipUpdateDocument
    {
        public string Id { get; set; }
        public DateTime? AssignmentDate { get; set; }
        public string Comments { get; set; }
        public string CompletedDate { get; set; }
        public string ContactedDate { get; set; }
        public string ProfilingCampaignSourceId { get; set; }
        public string StatusCode { get; set; }

        public string CreatedBy { get; set; }
    }
}
