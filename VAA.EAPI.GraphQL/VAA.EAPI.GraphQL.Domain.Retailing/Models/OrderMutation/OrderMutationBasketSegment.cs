namespace VAA.EAPI.GraphQL.Domain.Retailing.Models.OrderMutation
{
    public class OrderMutationBasketSegment
    {
        public string SegmentID { get; set; }
        public IEnumerable<OrderMutationBasketPassenger> Passengers { get; set; }
    }
}
