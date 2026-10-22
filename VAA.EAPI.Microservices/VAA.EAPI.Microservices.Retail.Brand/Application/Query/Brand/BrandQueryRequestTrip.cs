using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Brand.Application.Query.Brand
{
    public class BrandQueryRequestTrip
    {
        public string PointOfSale { get; set; }
        public string Source { get; set; }
        public IEnumerable<BrandQueryRequestSegment> Segments { get; set; }
        public IEnumerable<string> PassengerTypeCodes { get; set; }
    }
}
