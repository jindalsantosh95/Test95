using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OriginDestinationTrip
    {
        public string OriginDestinationId { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public IEnumerable<string> TripIds { get; set; }
    }
}
