using System.Collections.Generic;
using System;

namespace VAA.EAPI.Orchestration.Notifications.Domain.Entities
{
    public class Segment
    {
        public Carrier OperatingCarrier { get; set; }
        public Carrier MarketingCarrier { get; set; }
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime ScheduledDepartureUtcDateTime { get; set; }
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public DateTime ScheduledArrivalUtcDateTime { get; set; }
        public Airport Origin { get; set; }
        public Airport Destination { get; set; }
        public IEnumerable<Leg> Legs { get; set; }
        
    }
}