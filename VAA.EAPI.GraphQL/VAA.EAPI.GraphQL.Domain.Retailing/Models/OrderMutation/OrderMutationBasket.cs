namespace VAA.EAPI.GraphQL.Domain.Retailing.Models.OrderMutation
{
    public class OrderMutationBasket
    {
        public string RecordLocator { get; set; }
        public IEnumerable<OrderMutationBasketSegment> Segments { get; set; }
    }
}
