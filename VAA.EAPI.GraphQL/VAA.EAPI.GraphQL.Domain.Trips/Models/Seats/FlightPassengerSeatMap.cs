using HotChocolate;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    [GraphQLName("TripsFlightPassengerSeatMap")]
    public class FlightPassengerSeatMap
    {
        public string PassengerId { get; set; }
        public string CustomerId { get; set; }
        public string SegmentId { get; set; }
        public string SeatOfferId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SeatNumber { get; set; }
        public SeatPrice Price { get; set; }
        public string Pos { get; set; }       
        public InfantDetails LinkedInfant { get; set; }
        public string SelectedFareProductName { get; set; }
        public bool? IsConfirmed { get; set; }

        public static explicit operator FlightPassengerSeatMap((Infrastructure.Models.BasketMS.FlightPassengerSeatMap flightPassengerSeatMap, string seatOfferID, Booking booking) source)
        {
            var (flightPassengerSeatMap, seatOfferId, booking) = source;

            //if booking has the passenger seat details, use that to populate seat number
            var seatDetails = booking.SeatDetails.FirstOrDefault(s => s.PassengerId == flightPassengerSeatMap.PassengerId && s.SegmentId == flightPassengerSeatMap.SectorId);
            var person = booking.Passenger.FirstOrDefault(p => p.Id == flightPassengerSeatMap.PassengerId);
            return new()
            {
                PassengerId = flightPassengerSeatMap.PassengerId,
                CustomerId = person?.CustomerId,
                SegmentId = flightPassengerSeatMap.SectorId,
                FirstName = person?.FirstName,
                LastName = person?.LastName,
                SeatOfferId = seatOfferId,
                SeatNumber = flightPassengerSeatMap.SeatNumber.IsNullOrEmpty()  ? seatDetails?.SeatNumber ?? flightPassengerSeatMap.SeatNumber : flightPassengerSeatMap.SeatNumber,
                Price = new() { Amount = flightPassengerSeatMap.Price.Amount, Currency = flightPassengerSeatMap.Price.Currency },
                Pos = flightPassengerSeatMap.Pos,
                LinkedInfant = new() { PassengerId = flightPassengerSeatMap.LinkedInfantId }                
            };
        }
    }
}