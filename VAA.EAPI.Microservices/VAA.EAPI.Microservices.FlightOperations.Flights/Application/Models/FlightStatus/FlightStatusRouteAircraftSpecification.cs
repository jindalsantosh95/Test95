using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusRouteAircraftSpecification
    {
        public static explicit operator FlightStatusRouteAircraftSpecification?(DeltaAircraftSpecification? source) => source is null
            ? null
            : new FlightStatusRouteAircraftSpecification
            {
                Image = (FlightStatusRouteAircraftImage?)source.Image,
                Range = (FlightStatusRouteAircraftRange?)source.Range,
                CruisingSpeed = (FlightStatusRouteAircraftSpeed?)source.CruisingSpeed,
                SeatingCapacity = source.SeatingCapacity,
                EngineDescription = (FlightStatusRouteAircraftEngineDescription?)source.EngineDescription
            };

        public FlightStatusRouteAircraftImage? Image { get; set; }
        public FlightStatusRouteAircraftRange? Range { get; set; }
        public FlightStatusRouteAircraftSpeed? CruisingSpeed { get; set; }
        public int? SeatingCapacity { get; set; }
        public FlightStatusRouteAircraftEngineDescription? EngineDescription { get; set; }
    }
}
