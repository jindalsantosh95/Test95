using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight
{
    [GraphQLName("FlightDistanceBook")]
    public class FlightDistance
    {
        public string Unit { get; set; }
        public int? Distance { get; set; }
    }
}
