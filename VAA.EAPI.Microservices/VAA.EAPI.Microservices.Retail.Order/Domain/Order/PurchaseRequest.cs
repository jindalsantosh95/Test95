using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order
{
    public class PurchaseRequest
    {
        public string CorrelationId { get; set; }
        public string PaymentReferenceId { get; set; }
        public string BusinessUnit { get; set; }
        public string TransactionID { get; set; }
        public IEnumerable<Purchase> Purchases { get; set; }
        public string BrowserInfo { get; set; }
        public string OrderId { get; set; }
    }
}