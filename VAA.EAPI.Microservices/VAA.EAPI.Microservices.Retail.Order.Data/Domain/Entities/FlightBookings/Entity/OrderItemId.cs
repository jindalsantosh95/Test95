

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class OrderItemId
    {
        public string OrderItemID { get; set; }
        public string CustomerOrderNumber { get; set; }
        public string OrderPurchaseCount { get; set; }
        public string BillToCustomerName { get; set; }
        public string CreationUserIdentifier { get; set; }
        public string CreationGMTTimestamp { get; set; }
        public string SourceSystem { get; set; }
        public string EmailId { get; set; }
        public string OwnerAirline { get; set; }
        public string OwnerNonAirline { get; set; }
    }
}
