

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
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
