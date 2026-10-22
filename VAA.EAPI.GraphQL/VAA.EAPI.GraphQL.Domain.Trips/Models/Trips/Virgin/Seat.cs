using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsSeat")]
    public class Seat
    {
        public int? Row { get; set; }
        public string? Column { get; set; }
        public string?   Name { get; set; }
        public string? Type { get; set; }
        public IEnumerable<CabinClass>? ActualCabinClass { get; set; }

        public static explicit operator Seat((Infrastructure.Models.TripMS.Seat seat, IEnumerable<CabinRefData> cabinDetails) source)
        {
            var (seat, cabinDetails) = source;

            if (seat == null) return null;

            return new Seat()
            {
                ActualCabinClass = seat.ActualCabinClass?.Select(x => (CabinClass)(x, cabinDetails)),
                Column = seat.Column,
                Name = seat.Name,
                Row = seat.Row,
                Type = seat.Type
            };
        }

        public static explicit operator Seat((Infrastructure.Models.HolidaysMS.TripsCore.Seat Seat, IEnumerable<CabinRefData> cabinDetails) source)
        {
            var (seat, cabinDetails) = source;

            if (seat == null) return null;

            return new Seat()
            {
                ActualCabinClass = seat.ActualCabinClass?.Select(x => (CabinClass)(x, cabinDetails)),
                Column = seat.Column,
                Name = seat.Name,
                Row = seat.Row,
                Type = seat.Type
            };
        }
    }
}
