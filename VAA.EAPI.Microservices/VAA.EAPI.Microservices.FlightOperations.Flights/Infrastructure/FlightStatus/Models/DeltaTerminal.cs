namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaTerminal
    {
        public bool? DefaultTerminal { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public DeltaGate? Gate { get; set; }
    }
}
