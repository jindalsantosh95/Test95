using System;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain.Trips
{
    public class Leg
    {
        public string? Id { get; set; }
        public CarrierDetails Operating { get; set; }
        public CarrierDetails Marketing { get; set; }
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public AirportDetails Origin { get; set; }
        public AirportDetails Destination { get; set; }
        public CabinClass CabinClass { get; set; }
        public Aircraft Aircraft { get; set; }
    }
}