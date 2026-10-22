namespace VAA.EAPI.GraphQL.Domain.Retailing.Models.OrderMutation
{
    public class OrderMutationRequest
    {
        public string ShipmentRequestId { get; set; }
        public IEnumerable<OrderMutationBasket> OrderRequests { get; set; }
    }
}