using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryAirline")]
    public class Airline
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }
}