using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain
{
    public class UpdateOrderRequest
    {
        public string OrderId { get; set; }
        public OrderDocument OrderDocument { get; set; }
    }
}
