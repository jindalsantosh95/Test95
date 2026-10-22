namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaAirport
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? CityName { get; set; }
        public DeltaTerminal? Terminal { get; set; }
        public DeltaCity? City { get; set; }
        public DeltaCoordinate? Coordinate { get; set; }
    }
}
