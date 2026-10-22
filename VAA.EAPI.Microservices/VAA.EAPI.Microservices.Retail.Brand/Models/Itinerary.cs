
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class Itinerary
    {
        public bool IsValid { get; set; }
        public ICollection<Leg> Legs { get; set; } = new List<Leg>();
        public Amenities Amenities { get; set; }
        public IEnumerable<string> PassengerTypeCodes { get; set; }
    }
}
