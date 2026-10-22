using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Application.Models
{
    public class BookingInfo
    {
        public string RecordLocator { get; set; }
        public DateTime BookingDate { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<Segment> Segments { get; set; }

    }
}
