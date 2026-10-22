using System;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Application.Models
{
    public class Segment
    {
        public string SegmentID { get; set; }
        public Flight OperatingFlight { get; set; }
        public Flight MarketingFlight { get; set; }
        public DateTime Departure { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Cabin { get; set; }
        public EligibilityType Eligibility { get; set; }
        public string PopsNonEligibleReason { get; set; }

    }
}
