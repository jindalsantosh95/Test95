namespace VAA.EAPI.GraphQL.Domain.Search.Models.Holiday
{
    public class HolidayFlight
    {
        public string? Airline { get; set; }
        public string? AirlineName { get; set; }
        public string? CabinClass { get; set; }
        public string? DepartureAirport { get; set; }
        public string? DepartureAirportCode { get; set; }
        public string? ArrivalAirport { get; set; }
        public string? ArrivalAirportCode { get; set; }
        public IEnumerable<Sector> sectors { get; set; } = Enumerable.Empty<Sector>();
    }
}
