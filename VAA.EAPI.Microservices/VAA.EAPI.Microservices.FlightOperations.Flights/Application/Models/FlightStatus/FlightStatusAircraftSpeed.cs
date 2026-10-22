using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusAircraftSpeed
    {
        public static explicit operator FlightStatusAircraftSpeed?(DeltaAircraftSpeed? source) => source is null
            ? null
            : new FlightStatusAircraftSpeed { InMph = source.InMph, InKmph = source.InKmph };

        public int? InMph { get; set; }
        public int? InKmph { get; set; }
    }
}
