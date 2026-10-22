using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusAircraftRange
    {
        public static explicit operator FlightStatusAircraftRange?(DeltaAircraftRange? source) => source is null
            ? null
            : new FlightStatusAircraftRange { InMiles = source.InMiles, InKm = source.InKm };

        public int? InMiles { get; set; }
        public int? InKm { get; set; }
    }
}
