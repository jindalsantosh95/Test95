using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusRouteAircraftImage
    {
        public static explicit operator FlightStatusRouteAircraftImage?(DeltaAircraftImage? source) => source is null
            ? null
            : new FlightStatusRouteAircraftImage
            {
                DesktopUrl = source.DesktopUrl,
                MobileUrl = source.MobileUrl
            };

        public string? DesktopUrl { get; set; }
        public string? MobileUrl { get; set; }
    }
}
