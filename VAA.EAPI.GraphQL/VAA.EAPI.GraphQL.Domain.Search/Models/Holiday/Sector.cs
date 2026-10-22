namespace VAA.EAPI.GraphQL.Domain.Search.Models.Holiday
{
    public class Sector
    {
        public string? Airline { get; set; }
        public string? AirlineName { get; set; }
        public string? CabinClass { get; set; }
        public string? FlightNumber { get; set; }
        public int Stops { get; set; }
        public string? OperatingAirline { get; set; }
        public string? OperatingAirlineName { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string? DepartureAirport { get; set; }
        public string? DepartureAirportCode { get; set; }
        public string? ArrivalAirport { get; set; }
        public string? ArrivalAirportCode { get; set; }
    }
}
