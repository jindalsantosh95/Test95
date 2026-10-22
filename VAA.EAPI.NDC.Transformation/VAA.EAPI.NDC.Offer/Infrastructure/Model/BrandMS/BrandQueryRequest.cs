using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.BrandMS
{
    public class BrandQueryRequest
    {
        public string Currency { get; set; }
        public IEnumerable<BrandQueryRequestTrip> Itinerary { get; set; }
    }

}
