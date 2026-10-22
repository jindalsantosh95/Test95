using System;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders
{
    public class LocalAndUTCDateTime
    {
        public DateTime? LocalDateTime { get; set; }
        public DateTime? UtcDateTime { get; set; }
    }
}
