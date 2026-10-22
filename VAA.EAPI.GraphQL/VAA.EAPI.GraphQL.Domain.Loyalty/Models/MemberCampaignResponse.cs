using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class MemberCampaignResponse
    {
        public IEnumerable<MemberCampaign> MemberCampaigns { get; set; }
        public MemberCampaignResponse() { }
    }
}

