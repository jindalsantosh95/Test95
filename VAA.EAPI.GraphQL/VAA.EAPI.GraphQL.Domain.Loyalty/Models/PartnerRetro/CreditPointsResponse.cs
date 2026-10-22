using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class CreditPointsResponse : SkyPartnerRetroResponse
    {
        public IEnumerable<SegmentResponses> SegmentResponses { get; set; }
    }
}
