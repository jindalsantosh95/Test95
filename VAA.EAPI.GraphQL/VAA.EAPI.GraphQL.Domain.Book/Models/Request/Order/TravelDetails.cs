using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order
{
    [GraphQLName("insuranceTravelDetails")]
    public class TravelDetails
    {
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public Location Origin { get; set; }
        public Location Destination { get; set; }
    }
}