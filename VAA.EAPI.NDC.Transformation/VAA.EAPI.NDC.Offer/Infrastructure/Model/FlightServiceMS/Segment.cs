using VAA.EAPI.NDC.Offer.Domain.Model.Order;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.FlightServiceMS
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

        public LocalAndUTCDateTime ScheduledDepartureTime { get; set; }

        public LocalAndUTCDateTime ScheduledArrivalTime { get; set; }

        public string ClassOfService { get; set; }
    }
}
