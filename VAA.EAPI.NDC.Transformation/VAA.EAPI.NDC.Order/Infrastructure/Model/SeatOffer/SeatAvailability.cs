using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatOffer
{
    public class SeatAvailability
    {
        public IEnumerable<FlightSegmentSeatMap> FlightSegmentSeatMap { get; set; }

    }
}
