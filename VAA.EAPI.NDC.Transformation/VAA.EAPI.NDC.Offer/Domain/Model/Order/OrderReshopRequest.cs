using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class OrderReshopRequest
    {
        public string ReshopParameterCurrency { get; set; }
        public string ActionContext { get; set; }
        public OrderNumber OrderId { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<ReshopAction> OrderActions { get; set; }
    }
}