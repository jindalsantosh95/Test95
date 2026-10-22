using System;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class BookingReference
    {
        public string RecordLocatorId { get; set; }
        public string CarrierCode { get; set; }
        public DateTime BookingCreationDate { get; set; }
    }
}
