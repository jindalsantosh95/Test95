namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity
{
    public class ServiceStatus
    {
        public string CustomerOrderStatusTypeCode { get; set; }
        public string CustomerOrderStatusCreationGMTTimestamp { get; set; }
        public string CustomerStatusReason { get; set; }
        public string CustomerOrderItemSequenceNumber { get; set; }
    }
}
