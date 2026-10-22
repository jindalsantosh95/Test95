using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance
{
    public class PurchaseRequest
    {   
        public string PaymentReferenceId { get; set; }
        public string BusinessUnit { get; set; }
        public string TransactionID { get; set; }
        public IEnumerable<Purchase> Purchases { get; set; }
        public string BrowserInfo { get; set; }
        public string OrderId { get; set; }
    }
}