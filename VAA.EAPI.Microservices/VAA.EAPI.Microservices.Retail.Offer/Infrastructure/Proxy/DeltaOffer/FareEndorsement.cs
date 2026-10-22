using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FareEndorsement
    {
        public string BoxesText { get; set; }
        public Carrier Carrier { get; set; }
        public string DestinationAirportCode { get; set; }
        public string EndorsementText { get; set; }
        public string ExtendedFareCode { get; set; }
        public string FlightSegmentId { get; set; }
        public string FlightLegId { get; set; }
        public string TripId { get; set; }
        public string OriginAirportCode { get; set; }
        public IEnumerable<string> PenaltyIds { get; set; }
        public IEnumerable<Fee> Penalty { get; set; }
        public string ScheduledArrivalLocalTs { get; set; }
        public string ScheduledArrivalUtcTs { get; set; }
        public string ScheduledDepartureLocalTs { get; set; }
        public string ScheduledDepartureUtcTs { get; set; }
        public string TicketDesignator { get; set; }
        public Price SalesAdjustedPrice { get; set; }
        public string OriginCityCode { get; set; }
        public string DestinationCityCode { get; set; }
        public IEnumerable<string> BookingCodes { get; set; }
    }
}
