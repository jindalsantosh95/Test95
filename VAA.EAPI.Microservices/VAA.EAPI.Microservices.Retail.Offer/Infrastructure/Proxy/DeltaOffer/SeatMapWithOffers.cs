using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class SeatMapWithOffers
    {
        public FlightSegment PassengerItinerarySegment { get; set; }
        public IEnumerable<SeatDetailsWithOffer> SeatDetailsWithOffer { get; set; }
        public IEnumerable<SeatDisplay> SeatDisplay { get; set; }
    }
}
