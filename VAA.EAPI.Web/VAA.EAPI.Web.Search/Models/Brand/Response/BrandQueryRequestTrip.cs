using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Brand.Response
{
    public class BrandQueryRequestTrip
    {
        public string PointOfSale { get; set; }
        public string Source { get; set; }
        public IEnumerable<BrandQueryRequestSegment> Segments { get; set; }
    }
}