using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    [GraphQLName("BasketDepartureTimeRange")]
    public class DepartureTimeRange
    {
        public string EarliestTime { get; set; }
        public string LatestTime { get; set; }
    }
}
