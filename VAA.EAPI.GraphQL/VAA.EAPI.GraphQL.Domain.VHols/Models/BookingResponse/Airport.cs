namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Airport
    {
        public string? IataCode { get; set; }
        public string? Name { get; set; }
        public string? Terminal { get; set; }
        public GeoCoordinates? GeoCoordinates { get; set; }
    }
}

