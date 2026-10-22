using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class PromotionCampaignSearchResponse
    {
        public List<PromotionCampaignSearchData> PromotionCampaigns { get; set; }
    }
}