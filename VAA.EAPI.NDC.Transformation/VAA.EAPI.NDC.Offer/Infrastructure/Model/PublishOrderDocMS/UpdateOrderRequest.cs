using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.UpdateOrderMS.Domain
{
    public class UpdateOrderRequest
    {
        public OrderDocument OrderDocument { get; set; }
    }
}
