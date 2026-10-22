using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class AffinityRequest
    {
        public IEnumerable<OriginDestinationRequest> OriginDestinations { get; set; }
        public IEnumerable<AffinityRequestKeywordPreferences> KeywordPreferences { get; set; }
    }
}
