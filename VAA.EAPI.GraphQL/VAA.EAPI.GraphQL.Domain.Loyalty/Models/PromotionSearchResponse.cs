using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class PromotionSearchResponse
    {
        public List<PromotionSearchData> Promotions { get; set; }
    }
}