using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability
{
    public class SeatOfferUpdateRequest
    {
        public string SeatOfferID { get; set; }
        public IEnumerable<PassengerSeatInfo> PassengerSeats { get; set; }
    }
}
