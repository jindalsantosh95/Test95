using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusRouteAircraftRange
    {
        public static explicit operator FlightStatusRouteAircraftRange?(DeltaAircraftRange? source) => source is null
            ? null
            : new FlightStatusRouteAircraftRange { InMiles = source.InMiles, InKm = source.InKm };

        public int? InMiles { get; set; }
        public int? InKm { get; set; }
    }
}
