using System;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAssign
{
    public class TargetedMembershipDocCreateEventData
    {
        public TargetedMembershipDocument TargetedMembershipDocument { get; set; }
        public string CustomerId { get; set; }
    }
    public class TargetedMembershipDocument
    {
        public DateTime? AssignmentDate { get; set; }
        public string Comments { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime ContactedDate { get; set; }
        public string ProfilingCampaignSourceId { get; set; }
        public string StatusCode { get; set; }
        public string TargetedMembershipId { get; set; }
        public string createdBy { get; set; }
    }
}
