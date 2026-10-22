using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OrderData;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips
{
    public class TripDetailsRequest
    {
        public IEnumerable<TripDetails> TripDetails { get; set; }

        public static explicit operator TripDetailsRequest((FlightBookingsResponse response, string fcNumber) source)
        {
            var (response, fcNumber) = source;

            if (response.IsNullOrDefault() || response.FlightBookingsDetails.IsNullOrEmpty()) return null;

            return new()
            {
                TripDetails = source.response.FlightBookingsDetails.Select(b => (TripDetails)(b, fcNumber))
            };
        }
    }
}