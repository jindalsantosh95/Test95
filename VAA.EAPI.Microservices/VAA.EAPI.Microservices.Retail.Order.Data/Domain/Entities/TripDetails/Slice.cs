using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.TripDetails
{
    public class Slice
    {
        public string? Id { get; set; }
        public int DistanceMiles { get; set; }
        public int Ordinal { get; set; }
        public DateTime? ScheduledDepartureLocalDateTime { get; set; }
        public DateTime? ScheduledDepartureUtcDateTime { get; set; }
        public DateTime? ScheduledArrivalLocalDateTime { get; set; }
        public DateTime? ScheduledArrivalUtcDateTime { get; set; }
        public DateTime? ScheduledArrivalBasedOnDepartureLocalDateTime { get; set; }
        public AirportDetails? Origin { get; set; }
        public AirportDetails? Destination { get; set; }
        public IEnumerable<Segment>? Segments { get; set; }
        public string? CheckinStatus { get; set; }
        public string? TotalSliceDuration { get; set; }
        public bool? CheckInEligible { get; set; }
    }
}