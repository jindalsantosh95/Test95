using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderSearch
{
    [GraphQLName("Retailing.OrderSearchResponse")]
    public class OrderSearchResponse
    {
        public IEnumerable<CustomerDetail> CustomerDetails { get; set; }
        public IEnumerable<OrderInstructions> OrderInstructions { get; set; }
        public IEnumerable<SegmentDetails> SegmentDetails { get; set; }
        public BookingReferences BookingReferences { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
    }
}
