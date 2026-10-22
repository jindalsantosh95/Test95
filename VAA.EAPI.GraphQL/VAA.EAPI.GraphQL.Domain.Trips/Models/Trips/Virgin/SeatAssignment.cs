using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class SeatAssignment
    {
        public Seat? Seat { get; set; }

        public static explicit operator SeatAssignment((Infrastructure.Models.TripMS.SeatAssignment seatAssignment, IEnumerable<CabinRefData> cabinDetails) source)
        {
            var (seatAssignment, cabinDetails) = source;

            if (seatAssignment == null) return null;

            return new()
            {
                Seat = (Seat)(seatAssignment.Seat, cabinDetails)
            };
        }

        public static explicit operator SeatAssignment((Infrastructure.Models.HolidaysMS.TripsCore.SeatAssignment seatAssignment, IEnumerable<CabinRefData> cabinDetails) source)
        {
            var (seatAssignment, cabinDetails) = source;

            if (seatAssignment == null) return null;

            return new()
            {
                Seat = (Seat)(seatAssignment.Seat, cabinDetails)
            };
        }
    }
}
