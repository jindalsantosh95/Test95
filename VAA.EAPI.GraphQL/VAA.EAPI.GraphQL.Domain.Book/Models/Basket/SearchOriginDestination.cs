using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    [GraphQLName("BasketSearchOriginDestination")]
    public class SearchOriginDestination
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string DepartureDate { get; set; }
        public DepartureTimeRange DepartureTimeRange { get; set; }
    }
}
