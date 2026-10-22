using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusAircraftEngineDescription
    {
        public static explicit operator FlightStatusAircraftEngineDescription?(DeltaAircraftEngineDescription? source) => source is null
            ? null
            : new FlightStatusAircraftEngineDescription
            {
                EngineDescription1 = source.EngineDescription1,
                EngineDescription2 = source.EngineDescription2
            };

        public string? EngineDescription1 { get; set; }
        public string? EngineDescription2 { get; set; }
    }
}
