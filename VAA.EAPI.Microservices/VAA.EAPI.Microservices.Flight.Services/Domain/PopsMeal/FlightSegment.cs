using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Flight.Services.Domain.PopsMeal
{
    public class FlightSegment
    {
        public DateTime FlightDate { get; set; }
        public string SegmentId { get; set; }
        public FlightNumber FlightNumber { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
    }
}