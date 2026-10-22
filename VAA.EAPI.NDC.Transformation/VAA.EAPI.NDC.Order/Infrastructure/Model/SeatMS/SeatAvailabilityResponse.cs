using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS
{
    public class SeatAvailabilityResponse
    {
        public Parties Parties { get; set; }
        public Pos Pos { get; set; }        
        public string SeatOfferId { get; set; }        
        public string OfferExpirationTimeLimit { get; set; }
        public IEnumerable<FlightSegmentSeatMap> FlightSegmentSeatMap { get; set; }      
        public string OrderId { get; set; }
    }
}
