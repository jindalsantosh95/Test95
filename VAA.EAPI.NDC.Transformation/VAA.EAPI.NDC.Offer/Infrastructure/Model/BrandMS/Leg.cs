using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.BrandMS
{
    public class Leg
    {
        public string TravelTime { get; set; }
        public IEnumerable<Fare> Fares { get; set; }
    }
}
