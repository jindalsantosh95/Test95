using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderSearch
{
    [GraphQLName("Retailing.ProductSegmentAssociation")]
    public class ProductSegmentAssociation
    {
        public IEnumerable<string> FlightSegmentIds { get; set; }
    }
}