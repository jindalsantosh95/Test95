using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusTerminal
    {
        public static explicit operator FlightStatusTerminal?(DeltaTerminal? source) => source is null
            ? null
            : new FlightStatusTerminal
            {
                DefaultTerminal = source.DefaultTerminal,
                Gate = (FlightStatusGate?)source.Gate,
                Id = source.Id,
                Name = source.Name
            };

        public bool? DefaultTerminal { get; set; }
        public FlightStatusGate? Gate { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
    }
}
