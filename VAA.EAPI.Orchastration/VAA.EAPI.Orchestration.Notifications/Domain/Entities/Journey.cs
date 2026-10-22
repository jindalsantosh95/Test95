using System;
using System.Collections.Generic;

namespace VAA.EAPI.Orchestration.Notifications.Domain.Entities
{
    public class Journey
    {
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime ScheduledDepartureUtcDateTime { get; set; }
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public DateTime ScheduledArrivalUtcDateTime { get; set; }
        public Airport Origin { get; set; }
        public Airport Destination { get; set; }
        public IEnumerable<Segment> Segments { get; set; }

    }
}