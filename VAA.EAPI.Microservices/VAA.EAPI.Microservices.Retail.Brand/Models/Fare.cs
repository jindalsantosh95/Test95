using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class Fare
    {
        public string Product { get; set; }
        public ICollection<Segment> Segments { get; set; } = new List<Segment>();
        public Fees Fees { get; set; } = new Fees();

    }
}