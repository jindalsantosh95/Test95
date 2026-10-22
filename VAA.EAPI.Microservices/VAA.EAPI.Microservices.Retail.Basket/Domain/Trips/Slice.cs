using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain.Trips
{
    public class Slice
    {
        public string Id { get; set; }
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public AirportDetails Origin { get; set; }
        public AirportDetails Destination { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
    }
}
