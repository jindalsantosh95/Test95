using System;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.FlightServiceMS
{
    public class Segment
    {

        public string OriginDestId { get; set; }

        public string SegmentId { get; set; }

        public string MarketingCarrier { get; set; }

        public string MarketingFlightNumber { get; set; }

        public string OperatingCarrier { get; set; }

        public string OperatingFlightNumber { get; set; }

        public string OriginAirportCode { get; set; }

        public string DestinationAirportCode { get; set; }

        public DateTime? ScheduledDepartureTime { get; set; }

        public DateTime? ScheduledArrivalTime { get; set; }

        public string ClassOfService { get; set; }
    }
}
