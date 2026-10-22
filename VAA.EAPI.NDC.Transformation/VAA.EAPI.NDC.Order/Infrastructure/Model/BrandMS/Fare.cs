using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS
{
    public class Fare
    {
        public string Product { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public Fees Fees { get; set; }
    }
}
