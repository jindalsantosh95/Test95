using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class FlightInformation
    {
        public List<string> FlightSegmentIds { get; set; }
        public List<string> PassengerIds { get; set; }
    }
}
