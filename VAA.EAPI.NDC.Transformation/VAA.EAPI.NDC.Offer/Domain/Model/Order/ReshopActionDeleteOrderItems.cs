using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class ReshopActionDeleteOrderItems
    {
        public OrderItemNumber OrderItemId { get; set; }
        public IEnumerable<ProductCode> ProductRetention { get; set; }

    }
}
