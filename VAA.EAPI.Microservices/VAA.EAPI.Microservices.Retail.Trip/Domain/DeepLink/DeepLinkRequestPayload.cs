namespace VAA.EAPI.Microservices.Retail.Trip.Domain.DeepLink
{
    public class DeepLinkRequestPayload
    {
        public string? TripReservationId { get; set; }
        public Enum.DeeplinkOption DeeplinkOption { get; set; }
        public IEnumerable<string>? AdditionalData { get; set; }
        public string? POSCountryCode { get; set; }
        public string? FlightNumber { get; set; }
    }
}
