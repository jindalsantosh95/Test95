using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Brand.Response
{
    public class Fare
    {
        public string Product { get; set; }
        public ICollection<Segment> Segments { get; set; }
        public Fees Fees { get; set; } = new Fees();
    }
}