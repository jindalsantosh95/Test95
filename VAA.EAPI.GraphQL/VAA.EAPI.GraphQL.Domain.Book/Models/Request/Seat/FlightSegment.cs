using VAA.EAPI.GraphQL.Domain.Book.Models.Common;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat
{
    public class FlightSegment
    {
        public string SegmentId { get; set; }
        public string MarketingCarrier { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OperatingCarrier { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public string ClassOfService { get; set; }
        public UtcAndLocalDateTime ScheduledArrivalTime { get; set; }
        public UtcAndLocalDateTime ScheduledDepartureTime { get; set; }
        public string SegmentNumber { get; set; }
        public string FareBasis { get; set; }
        public string Aircraft { get; set; }
        public string BrandId { get; set; }
    }
}
