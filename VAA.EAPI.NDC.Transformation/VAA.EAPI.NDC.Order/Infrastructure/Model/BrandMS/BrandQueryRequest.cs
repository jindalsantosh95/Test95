using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS
{
    public class BrandQueryRequest
    {
        public string Currency { get; set; }
        public IEnumerable<BrandQueryRequestTrip> Itinerary { get; set; }
        public string PseudoCityCode { get; set; }
        public string AccountCode { get; set; }
    }

}
