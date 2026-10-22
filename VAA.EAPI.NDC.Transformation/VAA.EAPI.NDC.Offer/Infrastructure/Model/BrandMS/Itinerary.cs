using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.BrandMS
{
    public class Itinerary
    {
        public bool? IsValid { get; set; }
        public IEnumerable<Leg> Legs { get; set; }
        public Amenities Amenities { get; set; }
        public IEnumerable<string> PassengerTypeCodes { get; set; }
    }
}
