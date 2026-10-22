
using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.SSR
{
    [GraphQLName("GraphqlSSRMarketingFlight")]
    public class MarketingFlight
    {
        public string? OperatingSuffix { get; set; }
        public string? FlightNumber { get; set; }
        public IEnumerable<Airline>? MarketingAirlines { get; set; }

        public static explicit operator MarketingFlight(Infrastructure.Models.SSRMS.MarketingFlight source)
        {
            if (source == null)
                return null;

            return new()
            {
                FlightNumber = source.FlightNumber,
                MarketingAirlines = source.MarketingAirlines.Select(m=>(Airline)m),
                OperatingSuffix = source.OperatingSuffix
            };
        }
    }
}