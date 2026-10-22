using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders
{
    public class FlightProduct
    {
        public string ProductCode { get; set; }
        public string ProductClass { get; set; }
        public List<FlightSegment> Flights { get; set; }
    }
}
