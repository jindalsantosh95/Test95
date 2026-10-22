using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusAircraftSpecification
    {
        public static explicit operator FlightStatusAircraftSpecification?(DeltaAircraftSpecification? source) => source is null
            ? null
            : new FlightStatusAircraftSpecification
            {
                Image = (FlightStatusAircraftImage?)source.Image,
                Range = (FlightStatusAircraftRange?)source.Range,
                CruisingSpeed = (FlightStatusAircraftSpeed?)source.CruisingSpeed,
                SeatingCapacity = source.SeatingCapacity,
                EngineDescription = (FlightStatusAircraftEngineDescription?)source.EngineDescription
            };

        public FlightStatusAircraftImage? Image { get; set; }
        public FlightStatusAircraftRange? Range { get; set; }
        public FlightStatusAircraftSpeed? CruisingSpeed { get; set; }
        public int? SeatingCapacity { get; set; }
        public FlightStatusAircraftEngineDescription? EngineDescription { get; set; }
    }
}
