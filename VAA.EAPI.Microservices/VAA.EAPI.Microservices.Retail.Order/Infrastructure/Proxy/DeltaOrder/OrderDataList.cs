using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class OrderDataList
    {
        public List<Customer> Customers { get; set; }
        public List<PassengerItinerarySegment> PassengerItinerarySegment { get; set; }
        public List<Trip> Trips { get; set; }
        public List<FareByPassengerTypeCode> FareByPassengerTypeCode { get; set; }
    }
}
