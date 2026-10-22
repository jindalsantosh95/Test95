using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionSearch
{
    public class PromotionSearchResponse
    {
        public IEnumerable<Infrastructure.Models.Promotion> Promotions { get; set; }
    }
}
