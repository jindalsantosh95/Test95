using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.MemberProfileUpdate
{
    public class FlyingClubMemberDocument
    {
        public string FastTrackId { get; set; }
        public string InstantSilverCampaignId { get; set; }
        public string VAInstantGoldCampaignId { get; set; }
        public List<MemberAttributeDocument> MemberAttributes { get; set; }
        public string LastUpdatedBy { get; set; }

    }
}
