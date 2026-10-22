using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Flight
{
    public class Journey
    {
        public string JourneyId { get; set; }
        public string Duration { get; set; }
        public IEnumerable<string> FlightSegmentRefIds { get; set; }
    }
}
