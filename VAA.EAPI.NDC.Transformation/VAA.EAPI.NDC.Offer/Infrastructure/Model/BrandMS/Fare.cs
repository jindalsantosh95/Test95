using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.BrandMS
{
    public class Fare
    {
        public string Product { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public Fees Fees { get; set; }
    }
}
