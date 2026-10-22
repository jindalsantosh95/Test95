using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusRouteAircraftEngineDescription
    {
        public static explicit operator FlightStatusRouteAircraftEngineDescription?(DeltaAircraftEngineDescription? source) => source is null
            ? null
            : new FlightStatusRouteAircraftEngineDescription
            {
                EngineDescription1 = source.EngineDescription1,
                EngineDescription2 = source.EngineDescription2
            };

        public string? EngineDescription1 { get; set; }
        public string? EngineDescription2 { get; set; }
    }
}
