namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity
{
    public class ServicePayment
    {
        public string PurchaseId { get; set; }
        public string CustomerOrderItemSequenceNumber { get; set; }
        public string TotalOrderPurchaseAmount { get; set; }
        public string TotalOrderPurchaseTax { get; set; }
        public string CreationGMTTimestamp { get; set; }
    }
}
