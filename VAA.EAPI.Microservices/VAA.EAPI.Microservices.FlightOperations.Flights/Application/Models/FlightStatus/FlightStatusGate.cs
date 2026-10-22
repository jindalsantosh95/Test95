using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusGate
    {
        public static explicit operator FlightStatusGate?(DeltaGate? source) => source is null
            ? null
            : new FlightStatusGate { Id = source.Id };

        public string? Id { get; set; }
    }
}
