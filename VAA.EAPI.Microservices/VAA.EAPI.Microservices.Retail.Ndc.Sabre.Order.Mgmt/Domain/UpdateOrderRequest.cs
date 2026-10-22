using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain
{
    public class UpdateOrderRequest
    {
        public OrderDocument OrderDocument { get; set; }
    }
}
