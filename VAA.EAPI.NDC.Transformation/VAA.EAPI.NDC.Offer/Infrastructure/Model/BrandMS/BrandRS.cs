using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.BrandMS
{
    public class BrandRS
    {
        public IEnumerable<Itinerary> Itineraries { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
