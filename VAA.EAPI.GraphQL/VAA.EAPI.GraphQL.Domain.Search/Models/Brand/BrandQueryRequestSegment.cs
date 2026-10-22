using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand
{
    public class BrandQueryRequestSegment
    {
        public string? Carrier { get; set; }
        public int? FlightNumber { get; set; }
        public LocalDateTime? DepartureDate { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public int? Cabin { get; set; }
        public string? FareBasis { get; set; }
        public string? TicketDesignator { get; set; }
        public string? OperatingCarrier { get; set; }
        public string? FareFamily { get; set; }
    }
}
