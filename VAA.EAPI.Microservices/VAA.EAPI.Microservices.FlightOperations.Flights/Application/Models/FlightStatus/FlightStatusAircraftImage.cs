using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.FlightStatus.Models;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.FlightStatus
{
    public sealed class FlightStatusAircraftImage
    {
        public static explicit operator FlightStatusAircraftImage?(DeltaAircraftImage? source) => source is null
            ? null
            : new FlightStatusAircraftImage
            {
                DesktopUrl = source.DesktopUrl,
                MobileUrl = source.MobileUrl
            };

        public string? DesktopUrl { get; set; }
        public string? MobileUrl { get; set; }
    }
}
