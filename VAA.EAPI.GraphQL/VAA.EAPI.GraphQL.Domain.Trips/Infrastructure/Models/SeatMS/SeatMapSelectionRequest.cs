using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS
{
    public class SeatMapSelectionRequest
    {
        public string SeatOfferId { get; set; }
        public IEnumerable<PassengerSeatInfo> PassengerSeats { get; set; }

        public static explicit operator SeatMapSelectionRequest((Basket basket, string segmentId, string seatOfferId) source)
        {
            var (basket, segmentId, seatOfferId) = source;

            return new()
            {
                SeatOfferId = seatOfferId,
                PassengerSeats = basket.BookingDetails.Passenger.Select(p => (PassengerSeatInfo)(p, segmentId, basket))
            };
        }
    }
}