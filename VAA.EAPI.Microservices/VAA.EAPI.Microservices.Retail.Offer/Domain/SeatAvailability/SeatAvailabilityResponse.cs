using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.SeatAvailability
{
    public class SeatAvailabilityResponse
    {
        public List<FlightSegmentSeatMap> FlightSegmentSeatMap { get; set; }
    }
}
