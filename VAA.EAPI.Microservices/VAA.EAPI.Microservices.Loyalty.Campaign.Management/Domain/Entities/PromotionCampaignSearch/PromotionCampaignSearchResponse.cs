using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch
{
    public class PromotionCampaignSearchResponse
    {
        public IEnumerable<Infrastructure.Models.PromotionCampaign> PromotionCampaigns { get; set; }
    }
}
