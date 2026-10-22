using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    [GraphQLName("TripsTrip")]
    public class Trip
    {
        public int? TripId { get; set; }

        public static explicit operator Trip(Infrastructure.Models.TripMS.Trip source)
        {
            if (source == null)
                return null;

            return new Trip()
            {
                TripId = source.TripId
            };
        }
    }
}
