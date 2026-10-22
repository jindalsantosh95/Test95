using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryFare")]
    public class Fare
    {
        public string? PassengerType { get; set; }
        public int Quantity { get; set; }
        public Price? Price { get; set; }
    }
}