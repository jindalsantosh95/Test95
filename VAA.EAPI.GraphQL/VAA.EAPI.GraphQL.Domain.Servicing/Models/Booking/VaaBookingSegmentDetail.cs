using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking
{
    public class VaaBookingSegmentDetail
    {
        [GraphQLDescription("Segment Arrival Point")]
        public string? ArrivalPoint { get; set; }
        [GraphQLDescription("Segment Departure Point")]
        public string? DeparturePoint { get; set; }
        [GraphQLDescription("Segment Marketing Carrier")]
        public string? MarketingCarrier { get; set; }
        [GraphQLDescription("Segment Operating Carrier")]
        public string? OperatingCarrier { get; set; }
        [GraphQLDescription("Segment Class of Service ")]
        public string? ClassOfService { get; set; }
        [GraphQLDescription("Segment Departure Date")]
        public string? DepartureDate { get; set; }
        [GraphQLDescription("Segment Departure Time")]
        public string? DepartureTime { get; set; }
        [GraphQLDescription("Segment Flight Number")]
        public string? FlightNumber { get; set; }
    }
}
