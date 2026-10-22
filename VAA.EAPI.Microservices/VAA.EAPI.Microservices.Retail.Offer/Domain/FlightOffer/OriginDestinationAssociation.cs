using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Offer.Domain.FlightOffer
{
    public class OriginDestinationAssociation
    {
        public string OdLabel { get; set; }
        public OriginDestination OriginDestination { get; set; }
        public List<string> JourneyRefIds { get; set; }
    }
}
