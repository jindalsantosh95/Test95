using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    [GraphQLName("BasketFlightsearchRequest")]
    public class FlightsearchRequest
    {
        public IEnumerable<SearchOriginDestination> SearchOriginDestinations { get; set; }
        public bool isAwardSearch { get; set; }
        public string PromoCode { get; set; }
    }
}
