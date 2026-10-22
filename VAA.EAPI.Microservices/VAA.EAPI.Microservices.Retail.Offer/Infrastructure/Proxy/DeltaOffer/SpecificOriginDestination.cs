using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class SpecificOriginDestination
    {
        public Airport Destination { get; set; }
        public Airport Origin { get; set; }
        public IEnumerable<FlightSegment> FlightSegments { get; set; }
    }
}
