using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class SeatOfferCriteria
    {
        public string PassangerId { get; set; }
        public string FlightSegmentId { get; set; }
        public string FlightLegId { get; set; }
        public IEnumerable<Seat> SeatDetails { get; set; }
        public Dictionary<string, string> AdditionalCriteriaMap { get; set; }
    }
}
