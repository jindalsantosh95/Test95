using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval
{
    public class TargetedMembershipResponse
    {
        public IEnumerable<TargetedMembership> TargetedMembershipDocuments { get; set; }
        public string CustomerId { get; set; } = string.Empty;

        public TargetedMembershipResponse()
        {
            // Required for Serialisation
        }
    }
}
