using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval
{
    public class MemberCampaignResponse
    {
        public IEnumerable<MemberCampaign> MemberCampaigns { get; set; }
    }
}
