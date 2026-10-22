namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models
{
    public sealed class DeltaAircraftSpecification
    {
        public DeltaAircraftImage? Image { get; set; }
        public DeltaAircraftRange? Range { get; set; }
        public DeltaAircraftSpeed? CruisingSpeed { get; set; }
        public int? SeatingCapacity { get; set; }
        public DeltaAircraftEngineDescription? EngineDescription { get; set; }
    }
}
