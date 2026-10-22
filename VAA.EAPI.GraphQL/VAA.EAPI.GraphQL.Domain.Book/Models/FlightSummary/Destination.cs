using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryDestination")]
    public class Destination
    {
        public string? Code { get; set; }
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
        public string? AirportName { get; set; }
    }
}