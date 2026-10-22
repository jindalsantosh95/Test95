using System.Collections.Generic;
using System;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain.Trips
{
    public class Segment
    {
        public string Id { get; set; }
        public CarrierDetails OperatingCarrier { get; set; }
        public CarrierDetails MarketingCarrier { get; set; }
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public AirportDetails Origin { get; set; }
        public AirportDetails Destination { get; set; }
        public IEnumerable<Leg> Legs { get; set; }
        public string FareBasisCode { get; set; }
    }
}