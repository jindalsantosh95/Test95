using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Brand.Response
{
    public class Itinerary
    {
        public bool IsValid { get; set; }
        public ICollection<Leg> Legs { get; set; }
        public Amenities Amenities { get; set; }
    }
}