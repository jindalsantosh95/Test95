using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class Order
    {
        public string OrderId { get; set; }
        public List<BookingReference> BookingReference { get; set; }
        public OrderOwner OrderOwner { get; set; }
        public OrderProperties OrderProperties { get; set; }
        public OrderPricing OrderPricing { get; set; }
        public List<FormsOfPayment> FormsOfPayment { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public OrderDataList OrderDataList { get; set; }
        public List<AccountableDocument> AccountableDocuments { get; set; }
        public List<Warning> Warnings { get; set; }
    }
}
